using System.Collections.Generic;

namespace NubankCli.Core.Repositories.Api
{
    public class GetBillsResponse
    {
        public List<Bill> Bills { get; set; }
        public BillsLinks Links { get; set; }
    }

    public class BillsLinks
    {
        public Link Open { get; set; }
        public Link Future { get; set; }
    }
}
