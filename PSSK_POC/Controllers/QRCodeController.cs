using Microsoft.AspNetCore.Mvc;
using PSSK_POC.Contracts;
using PSSK_POC.Models;
using PSSK_POC.Services;
using System;

namespace PSSK_POC.Controllers
{
    [Route("api/[controller]")]
    public class QRCodeController : Controller
    {
        private readonly IQRCodeService _qRCodeService;
        private UserService _userService;
        public QRCodeController(IQRCodeService qRCodeService, UserService userService)
        {
            _qRCodeService = qRCodeService;
            _userService = userService;
        }

        [HttpGet("Validate")]
        public IActionResult ValidateQRCode([FromQuery] string clientSecret, string data)
        {
            var result = _userService.CheckQRCodeValid(clientSecret, data);
            return Ok(result);
        }
        
    }
}
