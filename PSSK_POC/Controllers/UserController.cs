﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using PSSK_POC.Models;
using PSSK_POC.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace PSSK_POC.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        public UserController(UserService userService)
        {
            UserService = userService;
        }

        public UserService UserService { get; }

        [HttpPost("Create")]
        public bool CreateUser([FromBody] PersonRequest person)
        {

            return UserService.CreateUser(person);
        }

        [HttpGet]
        public PersonResponse GetUser(string email, string userId, string authUserId)
        {
            return UserService.GetUser(email, userId, authUserId);
        }
        
        [HttpGet("List")]
        public List<PersonResponse> GetUserList()
        {
            return UserService.GetUsers();
        }
        
        [HttpGet("Nationalities")]
        public List<NationalityResponse> GetNationalities()
        {
            return UserService.GetNationalities();
        }

        [HttpGet("{userId}/QRCode")]
        public string GetQRCode(string userId)
        {
            return UserService.GetQRCode(userId);
        }

    }
}
