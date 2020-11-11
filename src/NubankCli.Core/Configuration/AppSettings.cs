namespace NubankCli.Core.Configuration
{
    public class AppSettings
    {
        public string NubankUrl { get; set; }
        public string MockUrl { get; set; }
        public bool EnableMockServer { get; set; }
        public string CurrentUser { get; set; }
    }
}
