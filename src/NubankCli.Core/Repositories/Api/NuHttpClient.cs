using Newtonsoft.Json;
using NubankSharp.Entities;
using NubankSharp.Extensions;
using RestSharp;
using RestSharp.Serializers.Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using RestRequest = RestSharp.Serializers.Newtonsoft.Json.RestRequest;

namespace NubankSharp.Repositories.Api
{
    public class NuHttpClient
    {
        public const string USER_AGENT = "NubankCli";

        private readonly RestClient _client = new();
        private readonly NuHttpClientLogging _logging;

        public string NubankUrl { get;  }
        public string NubankUrlMock { get; }
        public NuUser User { get; }

        public NuHttpClient(NuUser user, string nubankUrl, string nubankUrlMock = null, NuHttpClientLogging logging = null)
        {
            this.User = user;
            this.NubankUrl = nubankUrl;
            this.NubankUrlMock = nubankUrlMock;
            this._logging = logging;

            _client.AddHandler("application/json", new NewtonsoftJsonSerializer());
            _client.AddDefaultHeader("Content-Type", "application/json");
            _client.AddDefaultHeader("X-Correlation-Id", "WEB-APP.pewW9");
            _client.AddDefaultHeader("User-Agent", USER_AGENT);
        }

        private void AuthenticateIfLogged()
        {
            if (this.User.CertificateBase64 != null && (_client.ClientCertificates == null || _client.ClientCertificates.Count == 0))
            {
                var bytes = Convert.FromBase64String(this.User.CertificateBase64);
                SetCertificate(new X509Certificate2(bytes));
            }

            if (this.User.Token != null)
            {
                var authHeader = _client.DefaultParameters.FirstOrDefault(f => f.Type == ParameterType.HttpHeader && f.Name == "Authorization");
                if (authHeader == null)
                    SetToken(this.User.Token);
            }
        }

        public void SetCertificate(X509Certificate2 certificate2)
        {
            if (certificate2 != null)
            {
                _client.ClientCertificates ??= new X509CertificateCollection();
                _client.ClientCertificates.Add(certificate2);
            }
        }

        public void SetToken(string token)
        {
            if (token != null)
            {
                _client.AddDefaultHeader("Authorization", $"Bearer {this.User.Token}");
            }
        }

        public T Get<T>(string name, string url, out IRestResponse<T> response, params int[] allowedStatusCode)
        {
            this.AuthenticateIfLogged();

            url = GetUrl(url);
            _client.BaseUrl = new Uri(url);

            response = _client.Get<T>(new RestRequest());
            return GetResponseOrException(name, response, "GET", url, allowedStatusCode);
        }

        public T Get<T>(string name, string url, Dictionary<string, string> headers, out IRestResponse<T> response, params int[] allowedStatusCode)
        {
            this.AuthenticateIfLogged();

            var request = new RestRequest();
            url = GetUrl(url);
            _client.BaseUrl = new Uri(url);

            if (headers != null) 
            {
                headers.ToList().ForEach((KeyValuePair<string, string> header) =>
                {
                    request.AddHeader(header.Key, header.Value);
                });
             }

            response = _client.Get<T>(request);
            return GetResponseOrException(name, response, "GET", url, allowedStatusCode);
        }

        public T Post<T>(string name, string url, object body, out IRestResponse<T> response, params int[] allowedStatusCode)
        {
            this.AuthenticateIfLogged();

            var request = new RestRequest();
            url = GetUrl(url);
            _client.BaseUrl = new Uri(url);

            request.AddJsonBody(body);

            response = _client.Post<T>(request);

            return GetResponseOrException(name, response, "POST", url, allowedStatusCode);
        }

        public string Post(string name, string url, object body, Dictionary<string, string> headers, out IRestResponse response, params int[] allowedStatusCode)
        {
            this.AuthenticateIfLogged();

            var request = new RestRequest();
            url = GetUrl(url);
            _client.BaseUrl = new Uri(url);

            if (headers != null)
            {
                headers.ToList().ForEach((KeyValuePair<string, string> header) =>
                {
                    request.AddHeader(header.Key, header.Value);
                });
            }

            request.AddJsonBody(body);
            response = _client.Post(request);
            return GetResponseOrException(name, response, "POST", url, allowedStatusCode);
        }

        public T Post<T>(string name, string url, object body, Dictionary<string, string> headers, out IRestResponse<T> response, params int[] allowedStatusCode)
        {
            this.AuthenticateIfLogged();

            var request = new RestRequest();
            url = GetUrl(url);
            _client.BaseUrl = new Uri(url);

            if (headers != null)
            {
                headers.ToList().ForEach((KeyValuePair<string, string> header) =>
                {
                    request.AddHeader(header.Key, header.Value);
                });
            }

            request.AddJsonBody(body);
            response = _client.Post<T>(request);
            return GetResponseOrException(name, response, "POST", url, allowedStatusCode);
        }

        private string GetUrl(string url)
        {
            if (!string.IsNullOrWhiteSpace(this.NubankUrlMock))
            {
                var mockUrl = new Uri(this.NubankUrlMock);
                var builder = new UriBuilder(url)
                {
                    Scheme = mockUrl.Scheme,
                    Host = mockUrl.Host,
                    Port = mockUrl.Port,
                };

                url = builder.Uri.ToString();

                // TODO: uma forma melhor seria usar URI para fazer a subustituição do host
                //url = url.Replace(this.NubankUrl, this.NubankUrlMock);

                //url = url.Replace("https://prod-s0-webapp-proxy.nubank.com.br", this.NubankUrlMock);
                //url = url.Replace("https://prod-global-webapp-proxy.nubank.com.br", this.NubankUrlMock);
            }

            return url;
        }

        private T GetResponseOrException<T>(string name, IRestResponse<T> response, string verb, string url, params int[] allowedStatusCode)
        {
            SaveContentToFile(url, response, name);
            CheckStatus(response, verb, url, allowedStatusCode);
            return response.Data;
        }

        private string GetResponseOrException(string name, IRestResponse response, string verb, string url, params int[] allowedStatusCode)
        {
            SaveContentToFile(url, response, name);
            CheckStatus(response, verb, url, allowedStatusCode);
            return response.Content;
        }

        private void CheckStatus(IRestResponse response, string verb, string url, int[] allowedStatusCode)
        {
            var statusCode = (int)response.StatusCode;
            if ((statusCode == 0 || statusCode > 299) && !allowedStatusCode.Contains(statusCode))
                throw response.ErrorException ?? new Exception($"{verb} {url} - ({statusCode}) {response.ErrorMessage ?? response.StatusDescription}");
        }

        /// <summary>
        /// Save content in file
        /// </summary>
        /// <param name="content">Content to save</param>
        /// <param name="fileName">File location</param>
        public void SaveContentToFile(string url, IRestResponse response, string name)
        {
            if (_logging != null && this._logging != null)
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine($"-> REQUEST");
                stringBuilder.AppendLine();
                stringBuilder.AppendLine($"{response.Request.Method} {url}");
                var hasJson = false;
                var hasBody = false;
                
                foreach (var h in response.Request.Parameters)
                {
                    if (h.Type != ParameterType.RequestBody)
                    {
                        stringBuilder.AppendLine($"{h.Name}: {h.Value}");
                        if (h.Value.ToString().Contains("json"))
                            hasJson = true;
                    }
                    else
                    {
                        hasBody = true;
                    }
                }

                if (hasBody && hasJson)
                    stringBuilder.AppendLine(StringExtensions.BeautifyJson(response.Request.Body.Value.ToString()));
                else if (hasBody)
                    stringBuilder.AppendLine(response.Request.Body.Value.ToString());

                stringBuilder.AppendLine();

                stringBuilder.AppendLine($"-> RESPONSE");
                stringBuilder.AppendLine();
                stringBuilder.AppendLine($"StatusCode: {(int)response.StatusCode} ({response.StatusCode})");
                foreach (var h in response.Headers)
                {
                    stringBuilder.AppendLine($"{h.Name}: {h.Value}");
                }

                var fileName = $"{_logging.Folder}/{_logging.UserName}/{_logging.Scope}/Nubank-{name}";

                var ext = hasJson ? ".json" : null;
                fileName = $"{fileName}{ext}";

                if (!string.IsNullOrWhiteSpace(response.Content))
                {
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine(hasJson ? StringExtensions.BeautifyJson(response.Content) : response.Content);
                }

                var count = 0;
                while (File.Exists(fileName))
                {
                    count++;
                    fileName = Path.GetDirectoryName(fileName)
                            + Path.DirectorySeparatorChar
                            + Path.GetFileNameWithoutExtension(fileName)
                            + count.ToString()
                            + Path.GetExtension(fileName);
                }

                FileExtensions.CreateFolderIfNeeded(fileName);
                File.WriteAllText(fileName, stringBuilder.ToString());
            }
        }
    }
}
