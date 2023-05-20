using Newtonsoft.Json;
using System;

namespace NubankSharp.Repositories.Api
{
    public class BillSummary
    {
        [JsonProperty("due_date")]
        public DateTime DueDate { get; set; }

        [JsonProperty("close_date")]
        public DateTime CloseDate { get; set; }

        [JsonProperty("late_interest_rate")]
        public string LateInterestRate { get; set; }

        [JsonProperty("past_balance")]
        public long PastBalance { get; set; }

        [JsonProperty("late_fee")]
        public string LateFee { get; set; }

        [JsonProperty("effective_due_date")]
        public DateTime EffectiveDueDate { get; set; }

        [JsonProperty("total_balance")]
        public long TotalBalance { get; set; }

        [JsonProperty("interest_rate")]
        public string InterestRate { get; set; }

        [JsonProperty("interest")]
        public long Interest { get; set; }

        [JsonProperty("total_cumulative")]
        public long TotalCumulative { get; set; }

        [JsonProperty("paid")]
        public long Paid { get; set; }

        [JsonProperty("minimum_payment")]
        public long MinimumPayment { get; set; }

        [JsonProperty("open_date")]
        public DateTime OpenDate { get; set; }

        public long RemainingBalance { get; set; }
        public long RemainingMinimumPayment { get; set; }
    }
}
