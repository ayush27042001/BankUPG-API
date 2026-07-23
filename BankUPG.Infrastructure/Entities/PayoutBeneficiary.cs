using System;
using System.Collections.Generic;

namespace BankUPG.Infrastructure.Entities;

public partial class PayoutBeneficiary
{
    public long PayoutBeneficiaryId { get; set; }

    public int Mid { get; set; }

    public string BeneficiaryName { get; set; } = null!;

    public string? AccountNumber { get; set; }

    public string? Ifsccode { get; set; }

    public string? BankName { get; set; }

    public string? AccountType { get; set; }

    public string? UpiId { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public bool IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual Merchant MidNavigation { get; set; } = null!;

    public virtual ICollection<Payout> Payouts { get; set; } = new List<Payout>();
}
