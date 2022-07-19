using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PSSK_POC.Models;
using PSSK_POC.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PSSK_POC.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthenticationController : ControllerBase
    {
        public AuthenticationService AuthenticationService { get; }

        public AuthenticationController(AuthenticationService authenticationService)
        {
            AuthenticationService = authenticationService;
        }

        [HttpPost]
        [Route("Signup")]
        public ProfileResponse Signup([FromBody] ProfileRequest profile)
        {
            var result = AuthenticationService.Signup(profile);
            return result;
        }

        [HttpGet("Profile")]
        public ProfileResponse GetProfile([FromHeader] string Access_Key)
        {
            return AuthenticationService.GetProfile(Access_Key);
        }

        [HttpGet("Profile1")]
        public ProfileResponse GetProfile1([FromHeader] string Access_Key)
        {
            return AuthenticationService.GetProfile1(Access_Key);
        }

        [HttpGet("Login")]
        public ActionResult Login()
        {
            string response = AuthenticationService.GetLoginUrl();
            return new OkObjectResult(response);
        }
        
        [HttpGet("Logout")]
        public ActionResult Logout()
        {
            string response = AuthenticationService.GetLogoutUrl();
            return new OkObjectResult(response);
        }

        [HttpGet("UserMetadata/{userId}")]
        public ProfileResponse UserMetadata(string userId)
        {
            return AuthenticationService.GetUserMetadata(userId);
        }

        [HttpPut("UserMetadata/{userId}")]
        public bool UserMetadata([FromBody] UserMetadataRequest body, string userId)
        {
            return AuthenticationService.UpdateUserMetadata(body, userId);
        }


    }

}