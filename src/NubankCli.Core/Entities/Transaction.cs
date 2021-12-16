using Newtonsoft.Json;
using NubankSharp.Repositories.Api;
using System;
using System.Diagnostics;

namespace NubankSharp.Entities
{
    [DebuggerDisplay("{EventDate} {Name} {Number}/{Count} {Value}")]
    public class Transaction
    {
        public Guid Id { get; set; }
        public string Href { get; set; }
        public string Name { get; set; }
        public DateTime PostDate { get; set; }
        public DateTime EventDate { get; set; }
        public DateTime EventDateUtc { get; set; }
        public string Category { get; set; }
        public int Count { get; set; }
        public int Number { get; set; }
        public decimal Value { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string Type { get; set; }

        // Propriedade da pagamento da fatura anterior
        public bool IsBillPaymentLastBill { get; set; }

        // Propriedade da pagamento da fatura anterior ou adiantamentos
        [JsonIgnore]
        public bool IsBillPayment => IsByllPayment(Name, Type);

        [JsonIgnore]
        public string CardName { get; set; }

        [JsonIgnore]
        public Statement Statement { get; set; }
        [JsonIgnore]

        public Guid Target { get; internal set; }
        [JsonIgnore]
        public Guid Origin { get; internal set; }
        [JsonIgnore]
        public bool IsCorrelated => Target != default || Origin != default;

        public Transaction()
        {
        }

        public Transaction(BillTransaction billTransaction)
        {
            Id = billTransaction.Id;
            Href = billTransaction.Event?.Links?.Self?.Href ?? billTransaction.Href;
            Category = billTransaction.Category;
            Count = billTransaction.Charges == 1 ? 0 : billTransaction.Charges;
            Number = billTransaction.Charges > 1 ? billTransaction.Index + 1 : 0;
            Name = billTransaction.Title;
            PostDate = billTransaction.PostDate;
            EventDate = billTransaction.EventDate;
            EventDateUtc = billTransaction.EventDateUtc;
            Latitude = billTransaction.Event?.Details?.Lat ?? 0;
            Longitude = billTransaction.Event?.Details?.Lon ?? 0;
            Value = (billTransaction.Amount / 100m) * -1;
            IsBillPaymentLastBill = billTransaction.IsBillPaymentLastBill;
            Type = Enum.GetName(typeof(TransactionType), TransactionType.CreditEvent);
        }

        public Transaction(SavingFeed saving)
        {
            Id = saving.Id;
            Href = null;
            Category = null;
            Count = 0;
            Number = 0;
            Name = saving.GetCompleteTitle();
            PostDate = saving.PostDate;
            EventDate = saving.PostDate;
            EventDateUtc = saving.PostDate;
            Latitude = 0;
            Longitude = 0;
            Value = saving.GetValueWithSignal();
            Type = Enum.GetName(typeof(TransactionType), saving.TypeName);
            IsBillPaymentLastBill = saving.TypeName == TransactionType.BillPaymentEvent;
        }

        public string GetNameFormatted()
        {
            if (Count > 0)
                return $"{Name} {Number}/{Count}";

            return Name;
        }

        public static bool IsByllPayment(string name, string type)
        {
            return name == "Pagamento recebido" || type == "BillPaymentEvent";
        }
    }
}
