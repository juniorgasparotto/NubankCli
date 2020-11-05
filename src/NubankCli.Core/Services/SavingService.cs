using NubankCli.Core.Entities;
using NubankCli.Core.Extensions;
using NubankCli.Core.Repositories.Api;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NubankCli.Core.Services
{
    public class SavingService
    {
        private readonly NubankRepository client;
        private IEnumerable<Saving> _savings;

        private IEnumerable<Saving> Savings
        {
            get
            {
                if (_savings == null)
                    _savings = client.GetSavings();

                return _savings;
            }
        }

        public SavingService(NubankRepository nubankClient)
        {
            this.client = nubankClient;
        }

        public IEnumerable<Saving> GetItemsByMonth(DateTime? startFilter, DateTime? endFilter)
        {
            var start = (startFilter ?? Savings.Min(f => f.PostDate)).Date.GetDateBeginningOfMonth();
            var end = (endFilter ?? Savings.Max(f => f.PostDate)).Date.GetDateBeginningOfMonth();

            var allowTypes = new TransactionType[]
            {
                TransactionType.TransferInEvent,
                TransactionType.TransferOutEvent,
                TransactionType.TransferOutReversalEvent,
                TransactionType.BarcodePaymentEvent,
                TransactionType.BarcodePaymentFailureEvent,
                TransactionType.DebitPurchaseEvent,
                TransactionType.DebitPurchaseReversalEvent,
                TransactionType.BillPaymentEvent,
                TransactionType.DebitWithdrawalFeeEvent,
                TransactionType.DebitWithdrawalEvent,
            };

            var selecteds = Savings
                .Where(f => allowTypes.Contains(f.TypeName) && f.PostDate.GetDateBeginningOfMonth() >= start && f.PostDate.GetDateBeginningOfMonth() <= end)
                .OrderBy(f => f.PostDate)
                .ToList();

            var savings = new List<Saving>();
            foreach (var s in selecteds)
            {
                if (s.TypeName == TransactionType.BillPaymentEvent)
                {
                    // Modelo: "Cartão Nubank - R$ 1.987,06",
                    // 1) Divide na primeira ocorrencia de "-"
                    // 2) Remove os espaços; " R$ 1.987,06"
                    s.Amount ??= s.GetValueFromDetails();
                }

                savings.Add(s);
            }

            return savings;
        }
    }
}
