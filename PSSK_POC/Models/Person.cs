using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PSSK_POC.Models
{
    public class PersonRequest
    {
        public string AuthUserid { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public DateTime DateOfBirth { get; set; }
        [Required]
        public string Nationality { get; set; }
        public string PassportNumber { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        //public Address Address { get; set; }
    }
    public class PersonResponse
    {
        public string AuthUserid { get; set; }
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public DateTime DateOfBirth { get; set; }
        [Required]
        public string Nationality { get; set; }
        public string PassportNumber { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsDocumentReviewed { get; set; }
        public string QRCode { get; set; } = null;
        //public Address Address { get; set; }
    }
}
