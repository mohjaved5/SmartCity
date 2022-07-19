using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using PSSK_POC.Models;
using PSSK_POC.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PSSK_POC.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        public DocumentService DocumentService { get; }

        public DocumentController(DocumentService documentService)
        {
            DocumentService = documentService;
        }
        [HttpGet("DocumentTypes")]
        public List<DocumentTypes> GetDocumentTypes()
        {
            return DocumentService.GetDocumentTypes();
        }

        [HttpGet("ApproverTypes")]
        public List<ApproverTypes> GetApproverTypes()
        {
            return DocumentService.GetApproverTypes();
        }

        [HttpPost("Upload")]
        public bool UploadDocument([FromBody] AttachmentRequest attachment)
        {
            return DocumentService.UploadDocument(attachment);
        }

        [HttpGet]
        public List<AttachmentResponse> ListDocument(string userId)
        {
            return DocumentService.ListDocument(userId);
        }

        [HttpGet("Uri")]
        public Uri GetDocumentUrl(string documentName, string userId)
        {
            return DocumentService.GetDocumentUrl(documentName, userId);
        }
        
        [HttpPost("Review")]
        public bool ReviewDocument([FromBody]DocumentReview review)
        {
            return DocumentService.ValidateDocument(review);
        }
    }

}
