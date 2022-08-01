using AutoMapper;
using Microsoft.Azure.Cosmos;
using PSSK_POC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace PSSK_POC.Services
{
    public class UserService
    {

        // The Cosmos client instance
        private CosmosClient cosmosClient;

        // The database we will create
        private Database database;

        // The container we will create.
        private Container container;

        // The name of the database and container we will create
        private readonly string databaseId = "pssk";
        private readonly string containerId = "users";
        private readonly string nationalityContainerId = "nationalities";


        public AuthenticationService AuthenticationService { get; }
        public IMapper Mapper { get; }

        public UserService(AuthenticationService authenticationService, IMapper mapper)
        {
            AuthenticationService = authenticationService;
            Mapper = mapper;
        }
        public bool CreateUser(PersonRequest person)
        {
            try
            {
                var authProfile = AuthenticationService.GetUserMetadata(person.AuthUserid);
                person.Email = authProfile.email;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            GetClient();

            CreateDatabaseAsync();
            CreateContainerAsync();

            try
            {
                var personProfile = GetUser(person.Email, null);
                if (personProfile != null)
                {
                    UpdateUser(personProfile.Id, person);
                }
            }
            catch (Exception ex) when (ex.Message == "User Not Found")
            {
                AddNewUser(person).Wait();
            }
            return true;
        }
        public PersonResponse GetUser(string email, string userId, string authUserId = null)
        {
            GetClient();
            CreateDatabaseAsync();
            CreateContainerAsync();
            var response = GetUserDetails(email, userId, authUserId);

            return response;
        }
        public List<NationalityResponse> GetNationalities()
        {
            GetClient();
            CreateDatabaseAsync();
            CreateNationalityContainerAsync();
            var response = GetNationalitiesList();

            return response;
        }

        public string GetQRCode(string userId)
        {
            GetClient();
            CreateDatabaseAsync();
            CreateContainerAsync();
            var response = GetQRCodeImage(userId);

            if (response != null)
                return response.QRCode;
            else
                return null;
        }

        public List<PersonResponse> GetUsers()
        {
            GetClient();
            CreateDatabaseAsync();
            CreateContainerAsync();
            var response = GetUserList();

            return response;
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

        private void CreateContainerAsync()
        {
            // Create a new container
            this.container = this.database.CreateContainerIfNotExistsAsync(containerId, "/id").Result;
            Console.WriteLine("Created Container: {0}\n", this.container.Id);
        }

        private void CreateNationalityContainerAsync()
        {
            // Create a new container
            this.container = this.database.CreateContainerIfNotExistsAsync(nationalityContainerId, "/id").Result;
            Console.WriteLine("Created Container: {0}\n", this.container.Id);
        }

        private async Task AddNewUser(PersonRequest person1)
        {
            var person = Mapper.Map<PersonResponse>(person1);
            person.Id = Guid.NewGuid().ToString();
            try
            {
                // Read the item to see if it exists.  
                ItemResponse<PersonResponse> andersenFamilyResponse = await this.container.ReadItemAsync<PersonResponse>(person.Id, new PartitionKey(person.Id));
                Console.WriteLine("Item in database with id: {0} already exists\n", andersenFamilyResponse.Resource.Id);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Create an item in the container representing the Andersen family. Note we provide the value of the partition key for this item, which is "Andersen"
                ItemResponse<PersonResponse> andersenFamilyResponse = await this.container.CreateItemAsync<PersonResponse>(person, new PartitionKey(person.Id));

                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", andersenFamilyResponse.Resource.Id, andersenFamilyResponse.RequestCharge);
            }

        }

        public void UpdateUser(string userId, PersonRequest person)
        {
            // Read the item to see if it exists.  
            ItemResponse<PersonResponse> user = this.container.ReadItemAsync<PersonResponse>(userId, new PartitionKey(userId)).Result;
            var itemBody = user.Resource;
            // update FirstName
            itemBody.FirstName = person.FirstName;
            itemBody.LastName = person.LastName;
            itemBody.DateOfBirth = person.DateOfBirth;
            itemBody.Nationality = person.Nationality;
            itemBody.PassportNumber = person.PassportNumber;
            itemBody.IssueDate = person.IssueDate;
            itemBody.ExpiryDate = person.ExpiryDate;

            // replace/update the item with the updated content
            var result = this.container.ReplaceItemAsync<PersonResponse>(itemBody, itemBody.Id, new PartitionKey(itemBody.Id)).Result;

        }

        private PersonResponse GetUserDetails(string email, string userId, string authUserId)
        {
            var sqlQueryText = string.Empty;
            if (!string.IsNullOrEmpty(email))
                sqlQueryText = $"SELECT * FROM items where items.Email='{email}'";
            else if (!string.IsNullOrEmpty(userId))
                sqlQueryText = $"SELECT * FROM items where items.id='{userId}'";
            else
                sqlQueryText = $"SELECT * FROM items where items.AuthUserid='{authUserId}'";


            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            using FeedIterator<PersonResponse> queryResultSetIterator = this.container.GetItemQueryIterator<PersonResponse>(queryDefinition);

            List<PersonResponse> families = new List<PersonResponse>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<PersonResponse> currentResultSet = queryResultSetIterator.ReadNextAsync().Result;
                foreach (PersonResponse family in currentResultSet)
                {
                    families.Add(family);
                }
            }
            if (!families.Any())
                throw new Exception("User Not Found");
            return families.FirstOrDefault();
        }

        private QRCodeResponse GetQRCodeImage(string userId)
        {
            var sqlQueryText = $"SELECT i.QRCode FROM items i where i.id='{userId}'";

            List<QRCodeResponse> families = new List<QRCodeResponse>();

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            using FeedIterator<QRCodeResponse> queryResultSetIterator = this.container.GetItemQueryIterator<QRCodeResponse>(queryDefinition);
            if(queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<QRCodeResponse> currentResultSet = queryResultSetIterator.ReadNextAsync().Result;
                foreach (var family in currentResultSet)
                {
                    families.Add(family);
                }
            }
            return families.FirstOrDefault();
        }

        private List<PersonResponse> GetUserList()
        {
            var sqlQueryText = string.Empty;
            sqlQueryText = $"SELECT i.id, i.FirstName, i.LastName, i.Email FROM items i";

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            using FeedIterator<PersonResponse> queryResultSetIterator = this.container.GetItemQueryIterator<PersonResponse>(queryDefinition);

            List<PersonResponse> families = new List<PersonResponse>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<PersonResponse> currentResultSet = queryResultSetIterator.ReadNextAsync().Result;
                foreach (PersonResponse family in currentResultSet)
                {
                    families.Add(family);
                }
            }
            return families;
        }

        private List<NationalityResponse> GetNationalitiesList()
        {
            var sqlQueryText = string.Empty;
            sqlQueryText = $"SELECT i.id, i[\"Value\"] FROM items i";

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            using FeedIterator<NationalityResponse> queryResultSetIterator = this.container.GetItemQueryIterator<NationalityResponse>(queryDefinition);

            List<NationalityResponse> families = new List<NationalityResponse>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<NationalityResponse> currentResultSet = queryResultSetIterator.ReadNextAsync().Result;
                foreach (NationalityResponse family in currentResultSet)
                {
                    families.Add(family);
                }
            }
            return families;
        }

        public void MarkDocumentVerificationFalse(string userId)
        {
            // Read the item to see if it exists.  
            ItemResponse<PersonResponse> user = this.container.ReadItemAsync<PersonResponse>(userId, new PartitionKey(userId)).Result;
            var itemBody = user.Resource;
            // update FirstName
            itemBody.IsDocumentReviewed = false;

            // replace/update the item with the updated content
            var result = this.container.ReplaceItemAsync<PersonResponse>(itemBody, itemBody.Id, new PartitionKey(itemBody.Id)).Result;

        }

        public void UpdateQRCodeAndDocumentReviewedStatus(string userId, string qrCode, bool isDocumentReviewed)
        {
            // Read the item to see if it exists.  
            ItemResponse<PersonResponse> user = this.container.ReadItemAsync<PersonResponse>(userId, new PartitionKey(userId)).Result;
            var itemBody = user.Resource;
            // update FirstName
            itemBody.QRCode = qrCode;
            itemBody.IsDocumentReviewed = isDocumentReviewed;

            // replace/update the item with the updated content
            var result = this.container.ReplaceItemAsync<PersonResponse>(itemBody, itemBody.Id, new PartitionKey(itemBody.Id)).Result;
        }
    }
}
