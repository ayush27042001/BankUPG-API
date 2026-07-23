using System;

namespace BankUPG.Infrastructure.Entities;

public partial class BatchRefundItem
{
    public long BatchRefundItemId { get; set; }

    public long BatchRefundId { get; set; }

    public long TransactionId { get; set; }

    public long? RefundId { get; set; }

    public decimal Amount { get; set; }

    public string? Status { get; set; }

    public string? FailureReason { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual BatchRefund BatchRefund { get; set; } = null!;

    public virtual Transaction Transaction { get; set; } = null!;

    public virtual Refund? Refund { get; set; }
}
