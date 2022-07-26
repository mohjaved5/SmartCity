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
        public string username { get; set; }
        public string email { get; set; }
        public string given_name { get; set; }
        public string family_name { get; set; }
    }
    public class ProfileResponseAuth0
    {
        public string sub { get; set; }
        public bool email_verified { get; set; }
        public string nickname { get; set; }
        public string email { get; set; }
        public string given_name { get; set; }
        public string family_name { get; set; }
    }
}
