using System;

namespace BankUPG.Infrastructure.Entities;

public partial class Refund
{
    public long RefundId { get; set; }

    public int Mid { get; set; }

    public long? TransactionId { get; set; }

    public string? PayuId { get; set; }

    public string? MerchantReferenceId { get; set; }

    public string? RefundType { get; set; }

    public string? Source { get; set; }

    public string? BankArn { get; set; }

    public decimal? Amount { get; set; }

    public string? Status { get; set; }

    public string? PaymentAggregator { get; set; }

    public DateTime? RefundDate { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual Merchant MidNavigation { get; set; } = null!;

    public virtual Transaction? Transaction { get; set; }
}
