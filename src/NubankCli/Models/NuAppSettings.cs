namespace NubankSharp.Models
{
    public class NuAppSettings
    {
        public string NubankUrl { get; set; }
        public string MockUrl { get; set; }
        public bool EnableMockServer { get; set; }
        public bool EnableDebugFile { get; set; }
        public string CurrentUser { get; set; }
    }
}
