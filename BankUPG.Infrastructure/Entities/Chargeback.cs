using System;

namespace BankUPG.Infrastructure.Entities;

public partial class Chargeback
{
    public long ChargebackId { get; set; }

    public int Mid { get; set; }

    public long? TransactionId { get; set; }

    public string? PayuId { get; set; }

    public string? BankCaseNumber { get; set; }

    public string? CaseNumber { get; set; }

    public DateTime? ChargebackDate { get; set; }

    public DateTime? ReplyBefore { get; set; }

    public string? Status { get; set; }

    public string? ChargebackReason { get; set; }

    public string? ChargebackType { get; set; }

    public string? CloseReason { get; set; }

    public string? DocumentPath { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual Merchant MidNavigation { get; set; } = null!;

    public virtual Transaction? Transaction { get; set; }
}
