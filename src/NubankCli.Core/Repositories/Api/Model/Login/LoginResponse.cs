using QRCoder;
using System.Drawing;

namespace NubankCli.Core.Repositories.Api
{
    public class LoginResponse
    {
        public LoginResponse()
        {
            NeedsDeviceAuthorization = false;
        }

        public LoginResponse(string code)
        {
            NeedsDeviceAuthorization = true;
            Code = code;

            var qrGenerator = new QRCodeGenerator();
            _qrCodeData = qrGenerator.CreateQrCode(Code, QRCodeGenerator.ECCLevel.Q);
        }

        private readonly QRCodeData _qrCodeData;

        public bool NeedsDeviceAuthorization { get; }
        public string Code { get; }

        public string GetQrCodeAsAscii()
        {
            var qrCode = new AsciiQRCode(_qrCodeData);
            return qrCode.GetGraphic(1);
        }

        public Bitmap GetQrCodeAsBitmap()
        {
            var qrCode = new QRCode(_qrCodeData);
            return qrCode.GetGraphic(20);
        }
    }
}
