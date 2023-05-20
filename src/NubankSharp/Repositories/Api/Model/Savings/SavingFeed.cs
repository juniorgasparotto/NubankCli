using Newtonsoft.Json;
using NubankSharp.Entities;
using NubankSharp.Extensions;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace NubankSharp.Repositories.Api
{
    [DebuggerDisplay("{PostDate} - {Title} - {Amount} - {TypeName}")]
    public class SavingFeed
    {
        public Guid Id { get; set; }

        [JsonProperty("__typename")]
        [JsonConverter(typeof(TolerantEnumConverter))]
        public TransactionType TypeName { get; set; }

        public string Title { get; set; }
        public string Detail { get; set; }
        public DateTime PostDate { get; set; }
        public decimal? Amount { get; set; }
        public Account OriginAccount { get; set; }
        public Account DestinationAccount { get; set; }

        public decimal? GetValueFromDetails()
        {
            var match = Regex.Match(Detail, @"R\$\s*[-\d.,]+");

            if (match.Success)
            {
                string matchValue = match.Value;
                string valueStr = matchValue.Substring(2).Trim();
                return DecimalExtensions.ParseFromPtBr(valueStr).Value;
            }

            return null;
        }

        public string[] SplitDetails()
        {
            return Detail.Split(new char[] { '-', '\n' });
        }

        public decimal GetValueWithSignal()
        {
            return TypeName switch
            {
                TransactionType.TransferInEvent => Amount.Value,
                TransactionType.TransferOutEvent => (Amount ?? 0) * -1,
                TransactionType.BarcodePaymentEvent => (Amount ?? 0) * -1,
                TransactionType.BarcodePaymentFailureEvent => Amount.Value,
                TransactionType.DebitPurchaseEvent => (Amount ?? 0) * -1,
                TransactionType.DebitPurchaseReversalEvent => Amount.Value,
                TransactionType.BillPaymentEvent => (Amount ?? 0) * -1,
                TransactionType.CanceledScheduledTransferOutEvent => ThrowNotImplementedException<decimal>(),
                TransactionType.AddToReserveEvent => ThrowNotImplementedException<decimal>(),
                TransactionType.CanceledScheduledBarcodePaymentRequestEvent => ThrowNotImplementedException<decimal>(),
                TransactionType.RemoveFromReserveEvent => ThrowNotImplementedException<decimal>(),
                TransactionType.TransferOutReversalEvent => (Amount ?? 0) * -1,
                TransactionType.SalaryPortabilityRequestEvent => ThrowNotImplementedException<decimal>(),
                TransactionType.SalaryPortabilityRequestApprovalEvent => ThrowNotImplementedException<decimal>(),
                TransactionType.DebitWithdrawalFeeEvent => ThrowNotImplementedException<decimal>(),
                TransactionType.DebitWithdrawalEvent => Amount.Value,
                TransactionType.GenericFeedEvent => Title?.ToLower().Contains("transferência recebida") == true ? Amount ?? 0 : (Amount ?? 0) * -1,
                TransactionType.LendingTransferInEvent => Amount ?? 0,
                TransactionType.LendingTransferOutEvent => (Amount ?? 0) * -1,
                TransactionType.Unknown => 0,
                TransactionType.WelcomeEvent => 0,
                _ => ThrowNotImplementedException<decimal>(),
            };
        }

        public string GetCompleteTitle()
        {
            var detailsFirstValue = SplitDetails().FirstOrDefault().Trim();
            var separator = " - ";

            return TypeName switch
            {
                TransactionType.TransferInEvent => JoinIfNotNull(separator, Title, OriginAccount?.Name),
                TransactionType.TransferOutEvent => JoinIfNotNull(separator, Title, DestinationAccount?.Name),
                TransactionType.BarcodePaymentEvent => $"{Detail}",
                TransactionType.BarcodePaymentFailureEvent => JoinIfNotNull(separator, Title, Detail),
                TransactionType.DebitPurchaseEvent => $"{detailsFirstValue}",
                TransactionType.DebitPurchaseReversalEvent => JoinIfNotNull(separator, Title, Detail),
                TransactionType.BillPaymentEvent => JoinIfNotNull(separator, Title, "Cartão Nubank"),
                TransactionType.CanceledScheduledTransferOutEvent => ThrowNotImplementedException<string>(),
                TransactionType.AddToReserveEvent => ThrowNotImplementedException<string>(),
                TransactionType.CanceledScheduledBarcodePaymentRequestEvent => ThrowNotImplementedException<string>(),
                TransactionType.RemoveFromReserveEvent => ThrowNotImplementedException<string>(),
                TransactionType.TransferOutReversalEvent => JoinIfNotNull(separator, Title, detailsFirstValue),
                TransactionType.SalaryPortabilityRequestEvent => ThrowNotImplementedException<string>(),
                TransactionType.SalaryPortabilityRequestApprovalEvent => ThrowNotImplementedException<string>(),
                TransactionType.DebitWithdrawalFeeEvent => ThrowNotImplementedException<string>(),
                TransactionType.DebitWithdrawalEvent => detailsFirstValue,
                TransactionType.GenericFeedEvent => JoinIfNotNull(separator, Title, detailsFirstValue),
                TransactionType.LendingTransferInEvent => Title,
                TransactionType.LendingTransferOutEvent => Title,
                TransactionType.Unknown => null,
                TransactionType.WelcomeEvent => null,
                _ => ThrowNotImplementedException<string>(),
            };
        }

        private T ThrowNotImplementedException<T>()
        {
            throw new NotImplementedException($"Não foi encontrado um mapeamento para o tipo '{TypeName}' que foi encontrado na transação '{Title} ({Detail})'");
        }

        public static string JoinIfNotNull(string separator, params string[] values)
        {
            return string.Join(separator, values.Where(f => f != null));
        }
    }

    public class Account
    {
        public string Name { get; set; }
    }


}
