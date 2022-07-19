using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PSSK_POC.Models
{
    public class ProfileResponse
    {
        public string sub { get; set; }
        public bool email_verified { get; set; }
        public string email { get; set; }
        public string name { get; set; }
        public UserMetadata User_Metadata { get; set; }
    }
}
