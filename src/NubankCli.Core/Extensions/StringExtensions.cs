using Newtonsoft.Json;
using NubankSharp.DTOs;
using NubankSharp.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace NubankSharp.Extensions
{
    public static class StringExtensions
    {
        public static string GetFileNameFromUrl(string url)
        {
            var decoded = HttpUtility.UrlDecode(url);

            if (decoded.IndexOf("?") is { } queryIndex && queryIndex != -1)
            {
                decoded = decoded.Substring(0, queryIndex);
            }

            return Path.GetFileName(decoded);
        }

        public static string BeautifyJson(string str)
        {
            var obj = JsonConvert.DeserializeObject(str);
            string json = JsonConvert.SerializeObject(obj, Formatting.Indented);
            return json;
        }
    }
}
