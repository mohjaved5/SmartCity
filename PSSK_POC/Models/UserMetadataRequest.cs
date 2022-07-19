using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PSSK_POC.Models
{
    public class UserMetadata
    {
        public string Passport { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string POBox { get; set; }
        public string Gender { get; set; }
        public string BirthCountry { get; set; }
        public string ResidenceCountry { get; set; }
        public string MaritalStatus { get; set; }
        public string SpouseName { get; set; }
        public string HighestEducation { get; set; }
        public string EmergencyContactPerson{ get; set; }
        public string EmergencyContactNumber { get; set; }
    }
    public class UserMetadataRequest
    {
        public UserMetadata user_metadata{ get; set; }
       
    }
    public class UserMetadataResponse
    {
        public UserMetadata user_metadata { get; set; }

    }
}
