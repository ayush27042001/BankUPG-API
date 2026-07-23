using System;
using System.Collections.Generic;

namespace BankUPG.Infrastructure.Entities;

public partial class BatchRefund
{
    public long BatchRefundId { get; set; }

    public int Mid { get; set; }

    public string? BatchReferenceId { get; set; }

    public int TotalItems { get; set; }

    public decimal TotalAmount { get; set; }

    public int ProcessedItems { get; set; }

    public int SuccessCount { get; set; }

    public int FailedCount { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual Merchant MidNavigation { get; set; } = null!;

    public virtual ICollection<BatchRefundItem> BatchRefundItems { get; set; } = new List<BatchRefundItem>();
}
