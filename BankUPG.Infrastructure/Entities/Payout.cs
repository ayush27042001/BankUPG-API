using System;

namespace BankUPG.Infrastructure.Entities;

public partial class Payout
{
    public long PayoutId { get; set; }

    public int Mid { get; set; }

    public long? BeneficiaryId { get; set; }

    public decimal Amount { get; set; }

    public string? Currency { get; set; }

    public string? Mode { get; set; }

    public string? Status { get; set; }

    public string? ReferenceId { get; set; }

    public string? UtrNumber { get; set; }

    public string? Narration { get; set; }

    public DateTime? ScheduledDate { get; set; }

    public DateTime? ProcessedDate { get; set; }

    public string? FailureReason { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual Merchant MidNavigation { get; set; } = null!;

    public virtual PayoutBeneficiary? Beneficiary { get; set; }
}
