using System;

namespace BankUPG.Infrastructure.Entities;

public partial class EmiPlan
{
    public int EmiPlanId { get; set; }

    public int Mid { get; set; }

    public string BankName { get; set; } = null!;

    public string? CardType { get; set; }

    public int Tenure { get; set; }

    public decimal InterestRate { get; set; }

    public decimal? MinAmount { get; set; }

    public bool IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual Merchant MidNavigation { get; set; } = null!;
}
