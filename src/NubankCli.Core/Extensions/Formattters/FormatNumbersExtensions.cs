using System;

namespace NubankCli.Core.Extensions.Formatters
{
    public static class FormatNumbersExtensions
    {
        public static string HumanizeDefault(this decimal value)
        {
            return string.Format("{0:C2}", value);
        }

        public static string HumanizeDefault(this double value)
        {
            return string.Format("{0:C2}", value);
        }

        public static string HumanizeDefault(this float  value)
        {
            return string.Format("{0:C2}", value);
        }
    }
}