using Newtonsoft.Json;
using System.Collections.Generic;

namespace NubankSharp.Repositories.Api
{
    public class GetSavingsAccountFeedResponse
    {
        public GetSavingsAccountFeedResponse()
        {
            Data = new DataResponse();
        }

        [JsonProperty("data")]
        public DataResponse Data { get; set; }
    
        public class DataResponse
        {
            public DataResponse()
            {
                Viewer = new ViewerResponse();
            }

            [JsonProperty("viewer")]
            public ViewerResponse Viewer { get; set; }
        }

        public class ViewerResponse
        {
            public ViewerResponse()
            {
                SavingsAccount = new SavingsAccount();
            }

            [JsonProperty("savingsAccount")]
            public SavingsAccount SavingsAccount { get; set; }
        }

        public class SavingsAccount
        {
            public SavingsAccount()
            {
                Feed = new List<SavingFeed>();
            }

            [JsonProperty("feed")]
            public List<SavingFeed> Feed { get; set; }
        }
    }
}
