using Newtonsoft.Json;

namespace NubankSharp.Repositories.Api
{
    public class GetBalanceResponse
    {
        public GetBalanceResponse()
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
                SavingsAccount = new CurrentSavingsBalance();
            }

            [JsonProperty("savingsAccount")]
            public CurrentSavingsBalance SavingsAccount { get; set; }
        }

        public class CurrentSavingsBalance
        {
            [JsonProperty("currentSavingsBalance")]
            public NetAmountBalance NetAmountBalance { get; set; }
        }

        public class NetAmountBalance
        {
            [JsonProperty("netAmount")]
            public decimal NetAmount { get; set; }
        }
    }
}
