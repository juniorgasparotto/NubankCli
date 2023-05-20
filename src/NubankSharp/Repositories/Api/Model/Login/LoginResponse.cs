using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;

namespace NubankSharp.Repositories.Api
{
    public class LoginResponse
    {
        public string Token { get; private set; }
        public string RefreshToken { get; private set; }
        public Dictionary<string, string> AutenticatedUrls { get; private set; }

        public LoginResponse(Dictionary<string, object> response)
        {
            // Set Tokens
            if (response.Keys.Any(x => x == "error"))
                throw new AuthenticationException(response["error"].ToString());

            if (!response.Keys.Any(x => x == "access_token"))
                throw new AuthenticationException("Unknow error occurred on trying to do login on Nubank using the entered credentials");
        
            Token = response["access_token"].ToString();

            if (response.ContainsKey("refresh_token"))
                RefreshToken = response["refresh_token"].ToString();

            // Set urls
            var listLinks = (JObject)response["_links"];
            var properties = listLinks.Properties();
            var values = listLinks.Values();
            this.AutenticatedUrls = listLinks
                .Properties()
                .Select(x => new KeyValuePair<string, string>(x.Name, (string)listLinks[x.Name]["href"]))
                .ToDictionary(key => key.Key, key => key.Value);
        }
    }
}
