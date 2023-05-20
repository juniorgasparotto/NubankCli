using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace NubankSharp.Repositories.Api
{
    public class EndPointApi
    {
        private readonly string DiscoveryUrl;
        private readonly string DiscoveryAppUrl;
        private readonly NuHttpClient _client;

        public Dictionary<string, string> TopLevelUrs { get; private set; }
        public Dictionary<string, string> AppUrls { get; private set; }
        public Dictionary<string, string> AutenticatedUrls => this._client.User.AutenticatedUrls;

        public string Login => GetTopLevelUrl("login");
        public string ResetPassword => GetTopLevelUrl("reset_password");
        public string Lift => GetAppUrl("lift");
        public string Token => GetAppUrl("token");
        public string GenCertificate => GetAppUrl("gen_certificate");
        public string Events => GetAutenticatedUrl("events");
        public string BillsSummary => GetAutenticatedUrl("bills_summary");
        public string GraphQl => GetAutenticatedUrl("ghostflame");

        public EndPointApi(NuHttpClient httpClient)
        {
            _client = httpClient;
            DiscoveryUrl = $"{httpClient.NubankUrl}/api/discovery";
            DiscoveryAppUrl = $"{httpClient.NubankUrl}/api/app/discovery";
        }

        public string GetTopLevelUrl(string key)
        {
            if (TopLevelUrs == null)
                TopLevelUrs = Discover();
            return GetEndPoint(key, TopLevelUrs);
        }

        public string GetAppUrl(string key)
        {
            if (AppUrls == null)
                AppUrls = DiscoverApp();
            return GetEndPoint(key, AppUrls);
        }

        public string GetAutenticatedUrl(string key)
        {
            return GetEndPoint(key, AutenticatedUrls);
        }

        private Dictionary<string, string> Discover()
        {
            return _client.Get<Dictionary<string, string>>(nameof(DiscoveryUrl), DiscoveryUrl, out _);
        }

        private Dictionary<string, string> DiscoverApp()
        {
            var response = _client.Get<Dictionary<string, object>>(nameof(DiscoveryAppUrl), DiscoveryAppUrl, out _);

            return response
                .Where(x => x.Value is string)
                .Select(x => new KeyValuePair<string, string>(x.Key, x.Value.ToString()))
                .ToDictionary(x => x.Key, x => x.Value.ToString());
        }

        public static string GetEndPoint(string key, Dictionary<string, string> source)
        {
            if (!source.ContainsKey(key))
                return null;

            return source[key];
        }
    }
}