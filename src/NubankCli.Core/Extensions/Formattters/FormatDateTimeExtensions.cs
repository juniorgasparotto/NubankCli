using System;
using Humanizer;

namespace NubankCli.Core.Extensions.Formatters
{
    public static class FormatDateTimeExtensions
    {
        public const string FORMAT_DATE_UNIVERSAL = "yyyy-MM-dd HH:mm:ss";

        public static string HumanizeDefault(this DateTime date)
        {
            return date.Humanize(false);
        }

        public static string HumanizeDefault(this DateTime? date)
        {
            if (date == null)
                return "-";

            return date.Value.Humanize(false);
        }

        public static string ToShortDate(this DateTime? date)
        {
            if (date == null)
                return "-";

            return ToShortDate(date.Value);
        }

        public static string ToShortDate(this DateTime date)
        {
            return date.ToString("dd/MM/yyyy");
        }

        public static string ToLongDate(this DateTime? date)
        {
            return ToLongDate(date.Value);
        }

        public static string ToLongDate(this DateTime date)
        {
            if (date == null)
                return "-";

            return date.ToString("dd/MM/yyyy HH:mm:ss");
        }

        public static string ToLongDateNoSeconds(this DateTime date)
        {
            if (date == null)
                return "-";

            return date.ToString("dd/MM/yyyy HH:mm");
        }

        public static string ToUniversalString(this DateTime date)
        {            
            return date.ToString(FORMAT_DATE_UNIVERSAL);
        }
    }
}