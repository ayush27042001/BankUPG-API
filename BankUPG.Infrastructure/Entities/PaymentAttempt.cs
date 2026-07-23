using System;

namespace BankUPG.Infrastructure.Entities;

public partial class PaymentAttempt
{
    public long PaymentAttemptId { get; set; }

    public long OrderId { get; set; }

    public int Mid { get; set; }

    public long? TransactionId { get; set; }

    public string? PaymentMode { get; set; }

    public decimal Amount { get; set; }

    public string? Status { get; set; }

    public string? FailureReason { get; set; }

    public DateTime? AttemptDate { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual PaymentOrder Order { get; set; } = null!;

    public virtual Merchant MidNavigation { get; set; } = null!;

    public virtual Transaction? Transaction { get; set; }
}
