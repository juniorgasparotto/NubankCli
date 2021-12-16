using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace NubankSharp.Repositories.Api
{
    public class NuAuthApi
    {
        private readonly NuHttpClient restClient;
        private readonly EndPointApi endPointRepository;

        public NuAuthApi(NuHttpClient httpClient, EndPointApi endPointRepository)
        {
            this.restClient = httpClient;
            this.endPointRepository = new EndPointApi(httpClient);
        }

        #region LOGIN BY CERTIFICATE

        /// <summary>
        /// PARTE_1: Inicia a conexão da app com o Nubank. Após o sucesso da operação um e-mail será enviado com o código de ativação
        /// Após isso, devemos chamar o método que criar o certificado com o código recebido.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="appName"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public ConnectAppResponse ConnectApp(string userName, string password, string appName = null, string deviceId = null)
        {
            var rsa1 = RSA.Create();
            var rsa2 = RSA.Create();
            deviceId ??= Guid.NewGuid().ToString();
            appName ??= NuHttpClient.USER_AGENT;

            // https://newbedev.com/export-private-public-keys-from-x509-certificate-to-pem
            var loginCertResponse = new ConnectAppResponse()
            {
                UserName = userName,
                Password = password,
                PrivateKey = new string(PemEncoding.Write("PRIVATE KEY", rsa1.ExportPkcs8PrivateKey())),
                PrivateKeyCrypto = new string(PemEncoding.Write("PRIVATE KEY", rsa2.ExportPkcs8PrivateKey())),
                DeviceId = deviceId,
                PublicKey = new string(PemEncoding.Write("PUBLIC KEY", rsa1.ExportSubjectPublicKeyInfo())),
                PublicKeyCrypto = new string(PemEncoding.Write("PUBLIC KEY", rsa2.ExportSubjectPublicKeyInfo())),
                Model = $"{appName} ({deviceId})",
            };

            var body = new
            {
                login = userName,
                password = password,
                public_key = loginCertResponse.PublicKey,
                public_key_crypto =loginCertResponse.PublicKeyCrypto,
                model = loginCertResponse.Model,
                device_id = loginCertResponse.DeviceId
            };

            var response = restClient.Post<Dictionary<string, object>>(nameof(endPointRepository.GenCertificate), endPointRepository.GenCertificate, body, out var responseRest, 401);
            var wwwAuth = responseRest.Headers.FirstOrDefault(f => f.Name == "WWW-Authenticate").Value.ToString();
            loginCertResponse.EncryptedCode = Regex.Match(wwwAuth, "encrypted-code=\"(.+?)\"").Groups[1].Value;
            loginCertResponse.SentTo = Regex.Match(wwwAuth, "sent-to=\"(.+?)\"").Groups[1].Value;
            return loginCertResponse;
        }

        /// <summary>
        /// PARTE_2: Cria o certificado passando o código de ativação que você recebeu em seu e-mail.
        /// </summary>
        /// <param name="connectApp"></param>
        /// <param name="code"></param>
        /// <param name="autoAssignCertificate"></param>
        /// <returns></returns>
        public void GenerateAppCertificate(ConnectAppResponse connectApp, string code)
        {
            var body = new
            {
                login = connectApp.UserName,
                password = connectApp.Password,
                public_key = connectApp.PublicKey,
                public_key_crypto = connectApp.PublicKeyCrypto,
                model = connectApp.Model,
                device_id = connectApp.DeviceId,
                encrypted_code = connectApp.EncryptedCode,
                code = code
            };

            var url = endPointRepository.GenCertificate;

            //if (this._appSettings.EnableMockServer)
            //    url += "?mock=exchange_certs";

            var response = restClient.Post<Dictionary<string, string>>(nameof(endPointRepository.GenCertificate) + "ExchangeCerts", url, body, out _);

            var cert1Base64 = Encoding.ASCII.GetBytes(response["certificate"]);
            var cert2Base64 = Encoding.ASCII.GetBytes(response["certificate_crypto"]);

            var rsa1 = RSA.Create();
            rsa1.ImportFromPem(connectApp.PrivateKey.AsSpan());

            var rsa2 = RSA.Create();
            rsa2.ImportFromPem(connectApp.PrivateKeyCrypto.AsSpan());

            var certBytes = new X509Certificate2(cert1Base64, "").CopyWithPrivateKey(rsa1).Export(X509ContentType.Pkcs12);
            var certCryptoBytes = new X509Certificate2(cert2Base64, "").CopyWithPrivateKey(rsa2).Export(X509ContentType.Pkcs12);
            var certificate = new X509Certificate2(certBytes);
            var certificateBase64 = Convert.ToBase64String(certBytes);
            var certificateCryptoBase64 = Convert.ToBase64String(certCryptoBytes);

            //var result = new GenerateAppCertificateResponse(connectApp, certBytes, certCryptoBytes);
            this.restClient.SetCertificate(certificate);
            this.restClient.User.CertificateBase64 = certificateBase64;
            this.restClient.User.CertificateCryptoBase64 = certificateCryptoBase64;
        }

        /// <summary>
        /// PARTE_3: Faz o login informando os dados das respostas anteriores e obtendo as informações do usuário logado.
        /// </summary>
        /// <param name="beforeResponse"></param>
        /// <returns></returns>
        public void Login()
        {
            var body = new
            {
                grant_type = "password",
                client_id = "legacy_client_id",
                client_secret = "legacy_client_secret",
                login = this.restClient.User.UserName,
                password = this.restClient.User.Password
            };

            var response = restClient.Post<Dictionary<string, object>>(nameof(endPointRepository.Token), endPointRepository.Token, body, out _);
            var result = new LoginResponse(response);
            this.restClient.User.Token = result.Token;
            this.restClient.User.RefreshToken = result.RefreshToken;
            this.restClient.User.AutenticatedUrls = result.AutenticatedUrls;
        }

        #endregion

        #region LOGIN BY QRCODE

        public void Login(out bool needLoginValidation)
        {
            var body = new
            {
                client_id = "other.conta",
                client_secret = "yQPeLzoHuJzlMMSAjC-LgNUJdUecx8XO",
                grant_type = "password",
                login = this.restClient.User.UserName,
                password = this.restClient.User.Password
            };

            var response = restClient.Post<Dictionary<string, object>>(nameof(endPointRepository.Token), endPointRepository.Login, body, out _);
            var result = new LoginResponse(response);

            // Se não tiver o link do evento é pq não está logado ainda
            needLoginValidation = false;
            if (!result.AutenticatedUrls.ContainsKey("events"))
                needLoginValidation = true;

            this.restClient.User.Token = result.Token;
            this.restClient.User.RefreshToken = result.RefreshToken;
            this.restClient.User.AutenticatedUrls = result.AutenticatedUrls;
        }

        public void ValidateQRCode(string code)
        {
            var payload = new
            {
                qr_code_id = code,
                type = "login-webapp"
            };

            var response = restClient.Post<Dictionary<string, object>>(nameof(endPointRepository.Lift), endPointRepository.Lift, payload, out _);
            var result = new LoginResponse(response);
            this.restClient.User.Token = result.Token;
            this.restClient.User.RefreshToken = result.RefreshToken;
            this.restClient.User.AutenticatedUrls = result.AutenticatedUrls;
        }

        #endregion
    }
}