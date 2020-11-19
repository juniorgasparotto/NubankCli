using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Humanizer;

namespace NubankCli.Core.Extensions.Formatters
{
    public static class FormatStringExtensions
    {
        public const int MAX_LENGTH = 40;

        public static string Truncate(this string value, int length)
                => (value != null && value.Length > length) ? value.Substring(0, length) : value;

        public static string HumanizeDefault(this string text, int maxLength = MAX_LENGTH, string truncateIndicative = "...")
        {
            text = Regex.Replace(text, "(\n|\r|\t)+", " ");
            return text.Truncate(maxLength, truncateIndicative);
        }

        public static string TextIfNull<T>(this T value, string textIfNull = "-")
        {
            if (value is null)
                return textIfNull;

            return value.ToString();
        }

        public static string ToCamelCase(this string value)
        {
            return Char.ToLowerInvariant(value[0]) + value.Substring(1);
        }


        public static string ToLowerSeparate(this string str, char separate = '-')
        {
            if (!string.IsNullOrWhiteSpace(str))
            {
                var newStr = "";
                for (var i = 0; i < str.Length; i++)
                {
                    var c = str[i];
                    if (i > 0 && separate != str[i - 1] && char.IsLetterOrDigit(str[i - 1]) && char.IsUpper(c) && !char.IsUpper(str[i - 1]))
                        newStr += separate + c.ToString().ToLower();
                    else
                        newStr += c.ToString().ToLower();
                }

                return newStr;
            }

            return str;
        }

        public static string RemoveMoreThan2BreakLines(this string text)
        {
            text = Regex.Replace(text, $@"[{Environment.NewLine}]+", f =>
            {
                var count = 0;

                foreach (var c in f.Value)
                {
                    if (c == '\n')
                        count++;
                }

                return count < 3 ? f.Value : Environment.NewLine + Environment.NewLine;
            });
            return text;
        }

        public static string TrimFromPipe(this string text)
        {
            var strBuilder = new StringBuilder();
            var lines = text.Split(Environment.NewLine);

            var startLinePos = 0;
            var addSpaces = 0;
            foreach (var line in lines)
            {
                var onlyPipe = line.Trim();
                if (onlyPipe == "|")
                {
                    startLinePos = line.IndexOf("|");
                    addSpaces = 0;
                    continue;
                }

                if (onlyPipe == "||")
                {
                    addSpaces = line.IndexOf("|") - startLinePos;
                    startLinePos = line.IndexOf("|");
                    continue;
                }

                var countSpaces = 0;
                foreach (var c in line)
                {
                    if (c == ' ')
                        countSpaces++;
                    else
                        break;
                }

                string newLine;
                if (countSpaces >= startLinePos)
                {
                    newLine = line.Length >= startLinePos ? line.Substring(startLinePos) : line;
                }
                else
                {
                    newLine = line;
                }

                if (addSpaces > 0)
                    newLine = newLine.AddSpacesInAllLines(addSpaces);

                if (newLine.EndsWith("\r\n"))
                    strBuilder.Append(newLine);
                else
                    strBuilder.AppendLine(newLine);
            }

            return strBuilder.ToString();
        }

        public static string AddSpacesInAllLines(this string text, int spaces)
        {
            if (spaces == 0 || text?.Length == 0)
                return text;

            var strSpaces = new string(' ', spaces);
            var strBuilder = new StringBuilder();
            var lines = Regex.Split(text, Environment.NewLine);

            foreach (var line in lines)
                strBuilder.AppendLine($"{strSpaces}{line}");

            return strBuilder.ToString();
        }

        public static string JoinIfNotNull(string separator, params string[] values)
        {
            return string.Join(separator, values.Where(f => f != null));
        }
    }
}