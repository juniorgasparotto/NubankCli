using System;

namespace NubankSharp.Cli.Extensions.Formatters
{
    public static class FormatGuidExtensions
    {
        public static string HumanizeDefault(this Guid guid)
        {
            var split = guid.ToString().Split('-');
            return split[0];
        }
    }
}