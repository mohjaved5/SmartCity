﻿using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PSSK_POC.Contracts;
using PSSK_POC.Helpers;
using PSSK_POC.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static PSSK_POC.Common.Enums;

namespace PSSK_POC.Services
{
    public class DocumentService
    {
        // The Cosmos client instance
        private CosmosClient cosmosClient;

        // The database we will create
        private Database database;

        // The container we will create.
        private Container container;

        // The name of the database and container we will create
        private readonly string databaseId = "pssk";
        private readonly string documentTypeContainerId = "documentTypes";
        private readonly string approverTypeContainerId = "approverTypes";
        private static string _connectionString = "DefaultEndpointsProtocol=https;AccountName=pssk;AccountKey=3Bp0YmwfQyRpw6IZ8g1lTVv7dTUU2FlpR5vKOv+q84cuWAlzUiex1y7ZGbWAKS4IwedMKlDkweHa+AStRJ4DpQ==;EndpointSuffix=core.windows.net";
        private readonly IQRCodeService _qRCodeService;
        private IConfiguration _configuration;
        public DocumentService(UserService userService,
            IQRCodeService qRCodeService,
            IConfiguration configuration)
        {
            UserService = userService;
            _qRCodeService = qRCodeService;
            _configuration = configuration;
        }

        public UserService UserService { get; }

        public List<DocumentTypes> GetDocumentTypes()
        {
            GetClient();
            CreateDatabaseAsync();
            CreateDocumentContainerAsync();
            var response = GetDocumentTypeList();

            return response;
        }

        public List<ApproverTypes> GetApproverTypes()
        {
            GetClient();
            CreateDatabaseAsync();
            CreateApproverContainerAsync();
            var response = GetApproverTypeList();

            return response;
        }

        public bool UploadDocument(AttachmentRequest attachment)
        {
            var user = UserService.GetUser(null, attachment.UserId);
            if (user == null)
                throw new Exception("User not found");

            var containerName = user.Id;
            var containerClient = GetContainerClient(out BlobServiceClient blobServiceClient, containerName);

            var blobs = containerClient.GetBlobs(BlobTraits.All);
            if (blobs.Any(b => Convert.ToInt32(b.Metadata["DocumentTypeId"]) == attachment.DocumentTypeId))
                throw new Exception("Document Type Already available");

            // Get a reference to a blob
            var fileName = Guid.NewGuid() + "_" + attachment.Name;
            BlobClient blobClient = containerClient.GetBlobClient(fileName);
            var docId = Guid.NewGuid().ToString();
            byte[] imageBytes = Convert.FromBase64String(attachment.Document);
            var blob = blobServiceClient.GetBlobContainerClient(containerName).GetBlobs(BlobTraits.All, BlobStates.None, fileName);
            if (!blob.Any())
                using (Stream stream = new MemoryStream(imageBytes))
                {
                    blobClient.Upload(stream);
                    blobClient.SetMetadata(new Dictionary<string, string>() {
                        { "Id", docId },
                        {"FileExtension", attachment.FileExtension },
                        {"DisplayName", attachment.DisplayName},
                        {"Status", ((int)DocumentStatus.PendingApproval).ToString()},
                        {"DocumentTypeId", attachment.DocumentTypeId.ToString()}
                    });
                }

            UserService.MarkDocumentVerificationFalse(attachment.UserId);

            return true;
        }

        public bool DeleteDocument(string userId, string documentName)
        {
            var user = UserService.GetUser(null, userId);
            if (user == null)
                throw new Exception("User not found");

            var containerName = user.Id;
            var containerClient = GetContainerClient(out BlobServiceClient blobServiceClient, containerName);

            var blobs = containerClient.GetBlobs(BlobTraits.All, prefix: documentName);
            if (!blobs.Any())
                throw new Exception("Document not found");

            BlobClient blobClient = containerClient.GetBlobClient(documentName);
            blobClient.Delete();

            return true;
        }

        private BlobContainerClient GetContainerClient(out BlobServiceClient blobServiceClient, string containerName)
        {
            BlobContainerClient containerClient;
            blobServiceClient = new BlobServiceClient(_connectionString);
            //Create a unique name for the container
            try
            {
                // Create the container and return a container client object
                containerClient = blobServiceClient.CreateBlobContainerAsync(containerName).Result;
            }
            catch (Exception ex) when (ex.InnerException.Message.Contains("The specified container already exists."))
            {
                containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            }
            return containerClient;
        }

        public List<AttachmentResponse> ListDocument(string userId, int approverTypeId, bool returnBase64 = true)
        {
            List<AttachmentResponse> image = new List<AttachmentResponse>();
            var user = UserService.GetUser(null, userId);
            if (user == null)
                throw new Exception("User not found");

            var containerName = user.Id;
            List<DocumentTypes> documentTypes;

            if (approverTypeId == 0)
                documentTypes = GetDocumentTypes();
            else
                documentTypes = GetDocumentTypes().Where(d => d.ApproverTypeId == approverTypeId).ToList();

            var containerClient = GetContainerClient(out BlobServiceClient blobServiceClient, containerName);

            var blobs = containerClient.GetBlobs(BlobTraits.All);
            foreach (BlobItem blobItem in blobs)
            {
                BlobClient blobClient = containerClient.GetBlobClient(blobItem.Name);
                if (blobClient.ExistsAsync().Result)
                {
                    int docTypeId = Convert.ToInt32(blobItem.Metadata["documentTypeId"].ToString());
                    string document = string.Empty;
                    if (documentTypes.Select(d => d.Id).ToList().Contains(docTypeId))
                    {
                        if (returnBase64)
                        {
                            document = GetDocumentBase64(blobItem, blobClient, out StreamReader sr, out MemoryStream ms);
                        }
                        image.Add(new AttachmentResponse()
                        {
                            Id = blobItem.Metadata.ContainsKey("Id") ? blobItem.Metadata["Id"] : string.Empty,
                            Name = blobItem.Name,
                            FileExtension = blobItem.Metadata.ContainsKey("FileExtension") ? blobItem.Metadata["FileExtension"] : string.Empty,
                            DisplayName = blobItem.Metadata.ContainsKey("DisplayName") ? blobItem.Metadata["DisplayName"] : string.Empty,
                            DocumentTypeId = blobItem.Metadata.ContainsKey("DocumentTypeId") ? Convert.ToInt32(blobItem.Metadata["DocumentTypeId"].ToString()) : 0,
                            DocumentType = documentTypes.FirstOrDefault(d => d.Id == Convert.ToInt32(blobItem.Metadata["DocumentTypeId"].ToString())),
                            Url = blobClient.Uri.AbsoluteUri,
                            StatusId = blobItem.Metadata.ContainsKey("Status") ? Convert.ToInt32(blobItem.Metadata["Status"]) : 0,
                            Status = ((DocumentStatus)Convert.ToInt32(blobItem.Metadata["Status"])).ToString(),
                            Document = document
                        });
                    }
                }
            }
            return image;
        }

        public string GetDocument(string userId, string documentName)
        {
            string document = string.Empty;
            var user = UserService.GetUser(null, userId);
            if (user == null)
                throw new Exception("User not found");

            var containerName = user.Id;

            var containerClient = GetContainerClient(out BlobServiceClient blobServiceClient, containerName);
            var blobItem = containerClient.GetBlobs(BlobTraits.All, prefix: documentName).FirstOrDefault();
            BlobClient blobClient = containerClient.GetBlobClient(documentName);
            if (blobClient.ExistsAsync().Result)
            {
                document = GetDocumentBase64(blobItem, blobClient, out StreamReader sr, out MemoryStream ms);
            }
            return document;
        }

        private string GetDocumentBase64(BlobItem blobItem, BlobClient blobClient, out StreamReader sr, out MemoryStream ms)
        {
            var response = blobClient.DownloadStreamingAsync().Result;
            sr = new StreamReader(response.Value.Content);
            ms = new MemoryStream();
            sr.BaseStream.CopyTo(ms);
            var b64String = Convert.ToBase64String(ms.ToArray());
            new FileExtensionContentTypeProvider().TryGetContentType(blobItem.Name, out string contentType);
            var document = $"data:{contentType};base64," + b64String;
            return document;
        }


        public Uri GetDocumentUrl(string documentName, string userId)
        {
            Uri uri;
            var user = UserService.GetUser(null, userId);
            if (user == null)
                throw new Exception("User not found");

            var containerName = user.Id;

            var containerClient = GetContainerClient(out _, containerName);

            BlobClient blobClient = containerClient.GetBlobClient(documentName);
            uri = GetServiceSasUriForBlob(blobClient, null);

            return uri;
        }

        public bool ValidateDocument(DocumentReview review)
        {
            var user = UserService.GetUser(null, review.UserId);
            if (user == null)
                throw new Exception("User not found");

            var containerName = user.Id;

            var containerClient = GetContainerClient(out BlobServiceClient blobServiceClient, containerName);

            BlobClient blobClient = containerClient.GetBlobClient(review.DocumentName);
            if (blobClient.ExistsAsync().Result)
            {
                var blobItem = containerClient.GetBlobs(BlobTraits.All, BlobStates.None, review.DocumentName).FirstOrDefault();
                var metadata = blobItem.Metadata;
                var Status = blobItem.Metadata.ContainsKey("Status") ? Convert.ToInt32(blobItem.Metadata["Status"]) : 0;
                if (Status == 1)
                {
                    metadata["Status"] = review.StatusId.ToString();
                    blobClient.SetMetadata(metadata);
                }
            }

            // Generate QR Code for user if all documents are approved
            if (CheckMandatoryDocumentsApproved(review.UserId))
            {
                var qrCodeUri = CreateQRCodeUri(user);
                var qrCodeImage = _qRCodeService.GetQRCode(qrCodeUri);

                // update the qr code image for user
                UserService.UpdateQRCodeAndDocumentReviewedStatus(user.Id, qrCodeImage, true);
            }
            else
            {
                UserService.UpdateQRCodeAndDocumentReviewedStatus(user.Id, null, false);
            }


            return true;
        }

        private bool CheckMandatoryDocumentsApproved(string userId)
        {
            var allDocumentsList = ListDocument(userId, 0, false);
            var mandatoryDocumentsTypeIds = _configuration.GetSection("ApplicationSettings").GetSection("MandatoryDocumentTypes").Value.Split(',');

            // Check if all mandatory documents are uploaded
            if (mandatoryDocumentsTypeIds.Except(allDocumentsList.Select(x => x.DocumentTypeId.ToString())).Any())
            {
                return false;
            }

            // Check if all mandatory documents are approved
            if (allDocumentsList.Any(x => mandatoryDocumentsTypeIds.Contains(x.DocumentTypeId.ToString()) && !string.Equals(x.Status, DocumentStatus.Approved.ToString())))
                return false;

            return true;
        }

        private string CreateQRCodeUri(PersonResponse user)
        {
            var clientSecret = _configuration.GetSection("ApplicationSettings").GetSection("ClientSecret").Value;
            var qrCodeUri = _configuration.GetSection("ApplicationSettings").GetSection("QRCodeValidationAPI").Value;
            var baseApiUrl = _configuration.GetSection("ApplicationSettings").GetSection("APIBaseUrl").Value;
            var data = new QRCodeData
            {
                userId = user.Id,
                email = user.Email,
            };
            var symmetricCryptoKey = _configuration.GetSection("ApplicationSettings").GetSection("SymmetricCryptoKey").Value;
            var encryptedData = CryptographyHelper.EncryptString(symmetricCryptoKey, JsonConvert.SerializeObject(data));
            var encryptedClientSecret = CryptographyHelper.EncryptString(symmetricCryptoKey, clientSecret);
            qrCodeUri = String.Format(qrCodeUri, baseApiUrl, WebUtility.UrlEncode(encryptedClientSecret), WebUtility.UrlEncode(encryptedData));
            return qrCodeUri;
        }

        private static Uri GetServiceSasUriForBlob(BlobClient blobClient, string storedPolicyName = null)
        {
            // Check whether this BlobClient object has been authorized with Shared Key.
            if (blobClient.CanGenerateSasUri)
            {
                // Create a SAS token that's valid for one hour.
                BlobSasBuilder sasBuilder = new BlobSasBuilder()
                {
                    BlobContainerName = blobClient.GetParentBlobContainerClient().Name,
                    BlobName = blobClient.Name,
                    Resource = "b"
                };

                if (storedPolicyName == null)
                {
                    sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddHours(1);
                    sasBuilder.SetPermissions(BlobSasPermissions.Read |
                        BlobSasPermissions.Write);
                }
                else
                {
                    sasBuilder.Identifier = storedPolicyName;
                }

                Uri sasUri = blobClient.GenerateSasUri(sasBuilder);
                Console.WriteLine("SAS URI for blob is: {0}", sasUri);
                Console.WriteLine();

                return sasUri;
            }
            else
            {
                Console.WriteLine(@"BlobClient must be authorized with Shared Key 
                          credentials to create a service SAS.");
                return null;
            }
        }

        private void GetClient()
        {
            // Create a new instance of the Cosmos Client
            cosmosClient = new CosmosClient("AccountEndpoint=https://pssk-db1.documents.azure.com:443/;AccountKey=QxHJT4nqmevEUz8gIQZxKaaDFIrmkvSm9NFky5iJOqiHGhm2Ldh1PBdFeCWM3YoEkNkogZ8Xx2NreTZIKeahnA==;",
                new CosmosClientOptions()
                {
                    ConnectionMode = ConnectionMode.Gateway
                });
        }

        private void CreateDatabaseAsync()
        {
            // Create a new database
            this.database = this.cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId).Result;
        }

        private void CreateDocumentContainerAsync()
        {
            // Create a new container
            this.container = this.database.CreateContainerIfNotExistsAsync(documentTypeContainerId, "/id").Result;
            Console.WriteLine("Created Container: {0}\n", this.container.Id);
        }

        private void CreateApproverContainerAsync()
        {
            // Create a new container
            this.container = this.database.CreateContainerIfNotExistsAsync(approverTypeContainerId, "/id").Result;
            Console.WriteLine("Created Container: {0}\n", this.container.Id);
        }

        private List<DocumentTypes> GetDocumentTypeList()
        {
            var sqlQueryText = string.Empty;
            sqlQueryText = $"SELECT i.id, i.Type, i.ApproverTypeId FROM items i";

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            using FeedIterator<DocumentTypes> queryResultSetIterator = this.container.GetItemQueryIterator<DocumentTypes>(queryDefinition);

            List<DocumentTypes> families = new List<DocumentTypes>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<DocumentTypes> currentResultSet = queryResultSetIterator.ReadNextAsync().Result;
                foreach (DocumentTypes family in currentResultSet)
                {
                    families.Add(family);
                }
            }
            return families;
        }

        private List<ApproverTypes> GetApproverTypeList()
        {
            var sqlQueryText = string.Empty;
            sqlQueryText = $"SELECT i.id, i.Type FROM items i";

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            using FeedIterator<ApproverTypes> queryResultSetIterator = this.container.GetItemQueryIterator<ApproverTypes>(queryDefinition);

            List<ApproverTypes> families = new List<ApproverTypes>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<ApproverTypes> currentResultSet = queryResultSetIterator.ReadNextAsync().Result;
                foreach (ApproverTypes family in currentResultSet)
                {
                    families.Add(family);
                }
            }
            return families;
        }
    }
}
