using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PSSK_POC.Models
{
    public class ProfileRequest
    {
        public string client_id { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string connection { get; set; }
        public string name { get; set; }
        public UserMetadata user_metadata { get; set; }
    }
}
