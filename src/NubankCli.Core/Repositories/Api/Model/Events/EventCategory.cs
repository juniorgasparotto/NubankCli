using System.Runtime.Serialization;

namespace NubankCli.Core.Repositories.Api
{
    public enum EventCategory
    {
        Unknown,
        [EnumMember(Value = "transaction")]
        Transaction,
        [EnumMember(Value = "payment")]
        Payment,
        [EnumMember(Value = "bill_flow_paid")]
        BillFlowPaid,
        [EnumMember(Value = "welcome")]
        Welcome,
        [EnumMember(Value = "tutorial")]
        Tutorial,
        [EnumMember(Value = "customer_invitations_changed")]
        CustomerInvitationsChanged,
        [EnumMember(Value = "initial_account_limit")]
        InitialAccountLimit,
        [EnumMember(Value = "card_activated")]
        CardActivated,
        [EnumMember(Value = "transaction_reversed")]
        TransactionReversed,
        [EnumMember(Value = "account_limit_set")]
        AccountLimitSet,
        [EnumMember(Value = "customer_password_changed")]
        CustomerPasswordChanged,
        [EnumMember(Value = "bill_flow_closed")]
        BillFlowClosed,
        [EnumMember(Value = "customer_device_authorized")]
        CustomerDeviceAuthorized,
        [EnumMember(Value = "rewards_canceled")]
        RewardsCanceled,
        [EnumMember(Value = "rewards_signup")]
        RewardsSignup

    }
}
