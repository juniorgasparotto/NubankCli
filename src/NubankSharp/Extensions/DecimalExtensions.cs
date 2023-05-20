using System.Globalization;

namespace NubankSharp.Extensions
{
    public static class DecimalExtensions
    {
        public static decimal? ParseFromPtBr(string valueStr, bool throwException = true)
        {
            valueStr = valueStr.Replace("R$", "").Trim();
            var myCI = new CultureInfo("pt-BR", false).Clone() as CultureInfo;
            myCI.NumberFormat.CurrencyDecimalSeparator = ",";

            if (throwException)
                return decimal.Parse(valueStr, NumberStyles.Any, myCI);
            else if (decimal.TryParse(valueStr, NumberStyles.Any, myCI, out var ret))
                return ret;

            return null;
        }
    }
}
