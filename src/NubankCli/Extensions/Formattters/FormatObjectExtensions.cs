using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace NubankSharp.Cli.Extensions.Formatters
{
    public static class FormatObjectExtensions
    {
        public static List<IFormatConditionalBase> Formatters = new List<IFormatConditionalBase>
        {
            new FormatConditional<JValue>
            {
                Formatter = obj =>
                {
                    var formatter = Formatters.FirstOrDefault(f => f.CanFormat(obj.Value) == true);
                    if (formatter == null)
                        return obj?.Value?.ToString();

                    return formatter.FormatValue(obj.Value);
                }
            },

            new FormatConditional<Guid>
            {
                CanFormat = f => f is Guid || (f is string && Guid.TryParse(f.ToString(), out _)),
                Cast = f => new Guid(f.ToString()),
                Formatter = f => f.HumanizeDefault()
            },

            new FormatConditional<string> { Formatter = f => f?.Trim()?.HumanizeDefault() },
            new FormatConditional<decimal> { Formatter = f => f.HumanizeDefault() },
            new FormatConditional<double> { Formatter = f => f.HumanizeDefault() },
            new FormatConditional<float> { Formatter = f => f.HumanizeDefault() },
            new FormatConditional<DateTime> { Formatter = f => f.HumanizeDefault() },
            new FormatConditional<bool> { Formatter = f => f.HumanizeDefault() },
            new FormatConditional<IEnumerable>
            {
                Formatter = l =>
                {
                    var list = l.Cast<object>();
                    return String.Join(", ", list?.Select(f => f.Format()) ?? new string[] { }).Trim();
                }
            }
        };

        public static string Format(this object obj, params Type[] ignore)
        {
            string value = null;

            if (ignore?.Contains(obj?.GetType()) == false)
            {
                var formatter = Formatters.FirstOrDefault(f => f.CanFormat(obj) == true);

                if (formatter != null)
                    value = formatter.FormatValue(obj);
            }

            if (value == null)
                value = obj?.ToString();

            return value;
        }
    }
}