using Newtonsoft.Json;
using System;

namespace NubankSharp.Repositories.Api
{
    public class Event
    {
        public string Description { get; set; }
        [JsonConverter(typeof(TolerantEnumConverter))]
        public EventCategory Category { get; set; }
        public decimal Amount { get; set; }
        public decimal CurrencyAmount => Amount / 100;
        public DateTimeOffset Time { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Href { get; set; }
        public EventDetails Details { get; set; }

        [JsonProperty("_links")]
        public SelfLink Links { get; set; }
    }

    public class EventDetails
    {
        public decimal Lat { get; set; }
        public decimal Lon { get; set; }
    }
}