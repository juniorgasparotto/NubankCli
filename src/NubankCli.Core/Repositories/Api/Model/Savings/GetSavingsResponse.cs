using Newtonsoft.Json;
using System.Collections.Generic;

namespace NubankCli.Core.Repositories.Api
{
    public class GetSavingsResponse
    {
        public GetSavingsResponse()
        {
            Data = new DataResponse();
        }

        [JsonProperty("data")]
        public DataResponse Data { get; set; }

        [JsonIgnore]
        public List<Saving> Savings => Data.Viewer.SavingsAccount.Feed;
    }

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
            Feed = new List<Saving>();
        }

        [JsonProperty("feed")]
        public List<Saving> Feed { get; set; }
    }
}
