using System;
using System.Collections.Generic;

namespace BankUPG.Infrastructure.Entities;

public partial class BankAccountDetail
{
    public int BankAccountDetailId { get; set; }

    public int Mid { get; set; }

    public string BankHolderName { get; set; } = null!;

    public string BankAccountNumber { get; set; } = null!;

    public string Ifsccode { get; set; } = null!;

    public string? BankName { get; set; }

    public string? AccountType { get; set; }

    public bool? IsVerified { get; set; }

    public DateTime? VerifiedDate { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual Merchant MidNavigation { get; set; } = null!;
}
