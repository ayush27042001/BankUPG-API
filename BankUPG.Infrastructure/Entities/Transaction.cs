using System;
using System.Collections.Generic;

namespace BankUPG.Infrastructure.Entities;

public partial class Transaction
{
    public long TransactionId { get; set; }

    public int Mid { get; set; }

    public string? PayuId { get; set; }

    public string? MerchantReferenceId { get; set; }

    public string? CustomerEmail { get; set; }

    public string? CustomerPhone { get; set; }

    public string? CustomerName { get; set; }

    public string? PaymentMode { get; set; }

    public string? Source { get; set; }

    public decimal? Amount { get; set; }

    public string? Status { get; set; }

    public string? UpiReference { get; set; }

    public string? BankReference { get; set; }

    public DateTime? TransactionDate { get; set; }

    public long? OrderId { get; set; }

    public long? PaymentLinkId { get; set; }

    public long? SubscriptionId { get; set; }

    public long? InvoiceId { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual Merchant MidNavigation { get; set; } = null!;

    public virtual PaymentOrder? Order { get; set; }

    public virtual PaymentLink? PaymentLink { get; set; }

    public virtual Subscription? Subscription { get; set; }

    public virtual Invoice? Invoice { get; set; }

    public virtual ICollection<Refund> Refunds { get; set; } = new List<Refund>();

    public virtual ICollection<Chargeback> Chargebacks { get; set; } = new List<Chargeback>();

    public virtual ICollection<TransactionCharge> TransactionCharges { get; set; } = new List<TransactionCharge>();

    public virtual ICollection<PaymentAttempt> PaymentAttempts { get; set; } = new List<PaymentAttempt>();

    public virtual ICollection<BatchRefundItem> BatchRefundItems { get; set; } = new List<BatchRefundItem>();
}
