using Microsoft.AspNetCore.Mvc;
using PSSK_POC.Contracts;
using PSSK_POC.Models;
using System;

namespace PSSK_POC.Controllers
{
    [Route("[controller]")]
    public class QRCodeController : Controller
    {
        private readonly IQRCodeService _qRCodeService;
        public QRCodeController(IQRCodeService qRCodeService)
        {
            _qRCodeService = qRCodeService;
        }

        [HttpPost]
        public IActionResult CreateQrCode([FromBody] QRCodeData qRCodeData)
        {
            if(qRCodeData == null || string.IsNullOrEmpty(qRCodeData.qrCodeData))
            {
                throw new Exception("Invalid Paramaters");
            }
            var result = _qRCodeService.GetQRCode(qRCodeData.qrCodeData);
            return Ok(result);
        }
    }
}
