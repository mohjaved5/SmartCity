using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PSSK_POC.Models
{
    public class DocumentTypes
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public int ApproverTypeId { get; set; }
    }
}
