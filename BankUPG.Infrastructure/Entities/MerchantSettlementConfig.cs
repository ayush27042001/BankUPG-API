using System;

namespace BankUPG.Infrastructure.Entities;

public partial class MerchantSettlementConfig
{
    public int MerchantSettlementConfigId { get; set; }

    public int Mid { get; set; }

    public int SettlementT { get; set; }

    public string? SettlementCycleType { get; set; }

    public bool IsActive { get; set; }

    public DateTime? EffectiveFrom { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual Merchant MidNavigation { get; set; } = null!;
}
