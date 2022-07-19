using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PSSK_POC.Models
{
    public class AttachmentResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string FileExtension { get; set; }
        public int DocumentTypeId { get; set; }
        public DocumentTypes DocumentType { get; set; }
        public string Url { get; set; }
        public int StatusId { get; set; }
        public string Status { get; set; }
        public string Document { get; set; }

    }
}
