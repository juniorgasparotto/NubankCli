using System.Collections.Generic;
using System.Linq;

namespace NubankCli.Core.Repositories.Api
{
    public class EndpointsRepository
    {
        private const string DiscoveryUrl = "https://prod-s0-webapp-proxy.nubank.com.br/api/discovery";
        private const string DiscoveryAppUrl = "https://prod-s0-webapp-proxy.nubank.com.br/api/app/discovery";

        private readonly RestSharpClient _client;
        private Dictionary<string, string> _topLevelUrls;
        private Dictionary<string, string> _appUrls;

        public string Login => GetTopLevelUrl("login");
        public string ResetPassword => GetTopLevelUrl("reset_password");
        public string Events => GetAutenticatedUrl("events");
        public string BillsSummary => GetAutenticatedUrl("bills_summary");
        public string Lift => GetAppUrl("lift");
        public string GraphQl => GetAutenticatedUrl("ghostflame");
        public Dictionary<string, string> AutenticatedUrls { get; set; }

        public EndpointsRepository(RestSharpClient httpClient)
        {
            _client = httpClient;
            AutenticatedUrls = new Dictionary<string, string>();
        }

        public string GetTopLevelUrl(string key)
        {
            if (_topLevelUrls == null)
            {
                Discover();
            }
            return GetKey(key, _topLevelUrls);
        }

        public string GetAppUrl(string key)
        {
            if (_appUrls == null)
            {
                DiscoverApp();
            }
            return GetKey(key, _appUrls);
        }

        public string GetAutenticatedUrl(string key)
        {
            return GetKey(key, AutenticatedUrls);
        }

        private void Discover()
        {
            _topLevelUrls = _client.Get<Dictionary<string, string>>(DiscoveryUrl);
        }

        private void DiscoverApp()
        {
            var response = _client.Get<Dictionary<string, object>>(DiscoveryAppUrl);

            _appUrls = response
                .Where(x => x.Value is string)
                .Select(x => new KeyValuePair<string, string>(x.Key, x.Value.ToString()))
                .ToDictionary(x => x.Key, x => x.Value.ToString());
        }

        private static string GetKey(string key, Dictionary<string, string> source)
        {
            if (!source.ContainsKey(key))
                return null;

            return source[key];
        }
    }
}