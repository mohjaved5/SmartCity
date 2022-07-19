using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PSSK_POC.Models
{
    public class Document
    {
        public Guid Id { get; set; }
        public string Attachment { get; set; }
        public string DocumentName { get; set; }
    }
}
