using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NubankSharp.Services;
using System;
using System.Collections.Generic;
using System.IO;

namespace NubankSharp.Entities
{
    [Serializable]
    public class NuUser
    {
        public string UserName { get; }
        public string Password { get; private set; }

        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public string CertificateBase64 { get; set; }
        public string CertificateCryptoBase64 { get; set; }
        
        public Dictionary<string, string> AutenticatedUrls { get; set; }

        public NuUser(string userName, string password)
        {
            this.UserName = userName;
            this.Password = password;
        }

        public DateTime GetExpiredDate()
        {
            if (Token == null)
                return DateTime.MinValue;

            var jobject = JwtDecoderExtensions.GetClaims(Token);
            var exp = jobject["exp"].Value<int>();
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(exp);
            return dateTimeOffset.LocalDateTime;
        }

        public bool IsValid()
        {
            var exp = GetExpiredDate();
            if (exp > DateTime.Now)
                return true;

            return false;
        }

        public void CleanPassword()
        {
            Password = null;
        }


        public string GetLoginType()
        {
            return string.IsNullOrWhiteSpace(CertificateBase64) ? "QRCODE" : "CERTIFICATE";
        }
    }
}
