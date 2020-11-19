using Microsoft.Extensions.Options;
using NubankCli.Core.Configuration;
using RestSharp;
using RestSharp.Serializers.Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using RestRequest = RestSharp.Serializers.Newtonsoft.Json.RestRequest;

namespace NubankCli.Core.Repositories
{
    public class RestSharpClient
    {
        private readonly RestClient _client = new RestClient();
        private readonly AppSettings _appSettings;

        public bool EnableMockServer { get; }

        public RestSharpClient(IOptions<AppSettings> appSettings)
        {
            this._appSettings = appSettings.Value;
            this.EnableMockServer = this._appSettings.EnableMockServer;
            _client.ThrowOnAnyError = true;
            _client.FailOnDeserializationError = true;
            _client.ThrowOnDeserializationError = true;
            _client.AddHandler("application/json", new NewtonsoftJsonSerializer());
        }

        public T Get<T>(string url) where T : new()
        {
            url = GetUrl(url);
            _client.BaseUrl = new Uri(url);
            
            var response = _client.Get<T>(new RestRequest());
            return GetResponseOrException(response, "GET", url);
        }

        public T Get<T>(string url, Dictionary<string, string> headers) where T : new()
        {
            var request = new RestRequest();
            url = GetUrl(url);
            _client.BaseUrl = new Uri(url);

            headers.ToList().ForEach((KeyValuePair<string, string> header) =>
            {
                request.AddHeader(header.Key, header.Value);
            });

            var response = _client.Get<T>(request);
            return GetResponseOrException(response, "GET", url);
        }

        public T Post<T>(string url, object body) where T : new()
        {
            var request = new RestRequest();
            url = GetUrl(url);
            _client.BaseUrl = new Uri(url);

            request.AddJsonBody(body);

            var response = _client.Post<T>(request);
            return GetResponseOrException(response, "POST", url);
        }

        public T Post<T>(string url, object body, Dictionary<string, string> headers) where T : new()
        {
            var request = new RestRequest();
            url = GetUrl(url);
            _client.BaseUrl = new Uri(url);

            headers.ToList().ForEach((KeyValuePair<string, string> header) =>
            {
                request.AddHeader(header.Key, header.Value);
            });

            request.AddJsonBody(body);

            var response = _client.Post<T>(request);
            return GetResponseOrException(response, "POST", url);
        }

        private T GetResponseOrException<T>(IRestResponse<T> response, string verb, string url) where T : new()
        {
            var statusCode = (int)response.StatusCode;
            if (statusCode == 0 || statusCode > 299)
                throw response.ErrorException ?? new Exception($"{verb} {url} - ({statusCode}) {response.ErrorMessage ?? response.StatusDescription}");

            return response.Data;
        }

        private string GetUrl(string url)
        {
            if (this.EnableMockServer)
            {
                // TODO: uma forma melhor seria usar URI para fazer a subustituição do host
                url = url.Replace(this._appSettings.NubankUrl, this._appSettings.MockUrl);
                url = url.Replace("https://prod-s0-webapp-proxy.nubank.com.br", this._appSettings.MockUrl);
                url = url.Replace("https://prod-global-webapp-proxy.nubank.com.br", this._appSettings.MockUrl);
            }

            return url;
        }
    }
}
