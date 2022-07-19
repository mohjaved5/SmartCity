using IronBarCode;
using PSSK_POC.Contracts;
using System;

namespace PSSK_POC.Services
{
    public class QRCodeService: IQRCodeService
    {
        public string GetQRCode(string data)
        {
            var image = string.Empty;
            var genaratedBarCode = QRCodeWriter.CreateQrCode(data, 500, QRCodeWriter.QrErrorCorrectionLevel.Medium);
            byte[] bytes = genaratedBarCode.ToPngBinaryData();
            image = Convert.ToBase64String(bytes);
            return image;
        }
    }
}
