using System;

namespace NubankSharp.Repositories.Api
{
    public class NuHttpClientLogging
    {
        public string Folder { get; }
        public string UserName { get; }
        public string Scope { get; }

        public NuHttpClientLogging(string userName, string scope, string folder = null)
        {
            this.Folder = folder ?? "Logs";
            this.UserName = userName;
            this.Scope = $"{scope}-{DateTime.Now:yyyyMMddHHmmss}";
        }
    }
}
