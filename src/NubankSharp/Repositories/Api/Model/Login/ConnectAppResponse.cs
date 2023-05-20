using QRCoder;
using System.Drawing;
using System.Security.Cryptography;

namespace NubankSharp.Repositories.Api
{
    public class ConnectAppResponse
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string PrivateKey { get; set; }
        public string PrivateKeyCrypto { get; set; }
        public string PublicKey { get; set; }
        public string PublicKeyCrypto { get; set; }
        public string Model { get; set; }
        public string DeviceId { get; set; }
        public string EncryptedCode { get; set; }
        public string SentTo { get; set; }
    }
}
