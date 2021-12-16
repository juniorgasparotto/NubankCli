using System;

namespace NubankSharp.Extensions.Tables
{
    public class TableColumn<T>
    {
        public string Name { get; set; }
        public Func<T, string> ValueFormatter { get; set; }
        public Func<T, string> ValueFormatterWide { get; set; }
        public bool OnlyInWide { get; set; }
    }
}
