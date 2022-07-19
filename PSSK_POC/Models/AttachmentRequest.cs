using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PSSK_POC.Models
{
    public class AttachmentRequest
    {
        public string Id { get; set; }
        [Required]
        [MaxLength(800)]
        public string Name { get; set; }
        [Required]
        [MaxLength(800)]
        public string DisplayName { get; set; }
        [Required]
        [MaxLength(100)]
        public string FileExtension { get; set; }
        [Required]
        public string Document { get; set; }
        public int DocumentTypeId { get; set; }
        public string UserId { get; set; }
    }
}
