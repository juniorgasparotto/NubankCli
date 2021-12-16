using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace NubankSharp.Entities
{
    [DebuggerDisplay("Start: {Start} End: {End} Count: {Transactions.Count}")]
    public class Statement
    {
        public const string Version1_0 = "1.0.0";
        public string Version { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public Card Card { get; set; }
        public StatementType StatementType { get; set; }
        public List<Transaction> Transactions { get; set; }
    }
}
