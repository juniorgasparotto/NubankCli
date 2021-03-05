using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NubankCli.Core.Repositories.Api
{
    public class GetBillResponse
    {
        public Bill Bill { get; set; }
    }

    [DebuggerDisplay("{State} OpenDate: {Summary.OpenDate} CloseDate: {Summary.CloseDate}")]
    public class Bill
    {
        public Guid? Id { get; set; }
        public BillState State { get; set; }
        public BillSummary Summary { get; set; }

        [JsonProperty("line_items")]
        public List<BillTransaction> LineItems { get; set; }

        [JsonProperty("_links")]
        public SelfLink Links { get; set; }
    }

    [DebuggerDisplay("{PostDate} {Title} {Amount} {Index}/{Charges} ({Category})")]
    public class BillTransaction
    {
        public long Amount { get; set; }
        public int Index { get; set; }
        public string Title { get; set; }

        [JsonProperty("post_date")]
        public DateTime PostDate { get; set; }
        public DateTime EventDate
        {
            get
            {
                // Quando tem evento, a data vem no formato UTC e precisa converter pra pt-br
                if (Event != null)
                    return TimeZoneInfo.ConvertTimeFromUtc(EventDateUtc, Constants.BR_TIME_ZONE);

                // Quando NÃO tem evento é usado entao o PostDate QUE NÃO ESTÁ EM UTC, por isso não pode converter,
                // do contrário a data do evento seria um dia antes da compra
                return EventDateUtc;
            }
        }
        
        public DateTime EventDateUtc
        {
            get
            {
                // O nubank só expoe na timeline de eventos a primeira compra parcelada,
                // as demais parcelas não aparecem na timeline. Devido a isso, apenas
                // a primeira compra vai ter a data real da timeline, as demais vão com o horario 00:00
                // OBS: Se existir compras futuras no nubank, talvez aparece na timeline antes do débito real,
                //      isso pode ser um problema pois a data da compra será a da timeline e não a do débito. (VERIFICAR)
                if (Event?.Time != null && Index == 0)
                    return Event.Time.UtcDateTime;

                return PostDate;
            }
        }

        public Guid Id { get; set; }
        public string Category { get; set; }
        public int Charges { get; set; }
        public string Href { get; set; }

        public Event Event { get; set; }

    }
}
