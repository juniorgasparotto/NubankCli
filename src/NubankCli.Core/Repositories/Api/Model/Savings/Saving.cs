using Newtonsoft.Json;
using NubankCli.Core.Entities;
using NubankCli.Core.Extensions;
using System;
using System.Diagnostics;

namespace NubankCli.Core.Repositories.Api
{
    [DebuggerDisplay("{PostDate} - {Title} - {Amount} - {TypeName}")]
    public class Saving
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

        public decimal GetValueFromDetails()
        {
            return DecimalExtensions.ParseFromPtBr(Detail.Split('-')[1].Trim()).Value;
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
                TransactionType.CanceledScheduledTransferOutEvent => throw new NotImplementedException(),
                TransactionType.AddToReserveEvent => throw new NotImplementedException(),
                TransactionType.CanceledScheduledBarcodePaymentRequestEvent => throw new NotImplementedException(),
                TransactionType.RemoveFromReserveEvent => throw new NotImplementedException(),
                TransactionType.TransferOutReversalEvent => throw new NotImplementedException(),
                TransactionType.SalaryPortabilityRequestEvent => throw new NotImplementedException(),
                TransactionType.SalaryPortabilityRequestApprovalEvent => throw new NotImplementedException(),
                TransactionType.DebitWithdrawalFeeEvent => throw new NotImplementedException(),
                TransactionType.DebitWithdrawalEvent => throw new NotImplementedException(),
                TransactionType.Unknown => 0,
                TransactionType.WelcomeEvent => 0,
                _ => throw new NotImplementedException()
            };
        }

        public string GetCompleteTitle()
        {
            return TypeName switch
            {
                TransactionType.TransferInEvent => $"{Title} - {OriginAccount.Name}",
                TransactionType.TransferOutEvent => $"{Title} - {DestinationAccount.Name}",
                TransactionType.BarcodePaymentEvent => $"{Detail}",
                TransactionType.BarcodePaymentFailureEvent => $"{Title} - {Detail}",
                TransactionType.DebitPurchaseEvent => $"{Detail}",
                TransactionType.DebitPurchaseReversalEvent => $"{Title} - {Detail}",
                TransactionType.BillPaymentEvent => $"{Title} - Cartão Nubank",
                TransactionType.CanceledScheduledTransferOutEvent => throw new NotImplementedException(),
                TransactionType.AddToReserveEvent => throw new NotImplementedException(),
                TransactionType.CanceledScheduledBarcodePaymentRequestEvent => throw new NotImplementedException(),
                TransactionType.RemoveFromReserveEvent => throw new NotImplementedException(),
                TransactionType.TransferOutReversalEvent => throw new NotImplementedException(),
                TransactionType.SalaryPortabilityRequestEvent => throw new NotImplementedException(),
                TransactionType.SalaryPortabilityRequestApprovalEvent => throw new NotImplementedException(),
                TransactionType.DebitWithdrawalFeeEvent => throw new NotImplementedException(),
                TransactionType.DebitWithdrawalEvent => throw new NotImplementedException(),
                TransactionType.Unknown => null,
                TransactionType.WelcomeEvent => null,
                _ => throw new NotImplementedException()
            };
        }
    }

    public class Account
    {
        public string Name { get; set; }
    }
}
