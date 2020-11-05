namespace NubankCli.Core.Entities
{
    public enum TransactionType
    {
        Unknown,
        WelcomeEvent,
        TransferInEvent,
        TransferOutEvent,
        BarcodePaymentEvent,
        BarcodePaymentFailureEvent,
        CanceledScheduledTransferOutEvent,
        AddToReserveEvent,
        DebitPurchaseEvent,
        DebitPurchaseReversalEvent,
        BillPaymentEvent,
        CanceledScheduledBarcodePaymentRequestEvent,
        RemoveFromReserveEvent,
        TransferOutReversalEvent,
        SalaryPortabilityRequestEvent,
        SalaryPortabilityRequestApprovalEvent,
        DebitWithdrawalFeeEvent,
        DebitWithdrawalEvent,
        CreditEvent,

    }
}
