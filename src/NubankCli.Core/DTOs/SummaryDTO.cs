namespace NubankCli.Core.DTOs
{
    public  class SummaryDTO
    {
        public int CountIn { get; set; }
        public decimal ValueIn { get; set; }
        
        public int CountOut { get; set; }
        public decimal ValueOut { get; set; }

        public int CountTotal { get; set; }
        public decimal ValueTotal { get; set; }
    }
}
