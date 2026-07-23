using System;

namespace BankUPG.Infrastructure.Entities;

public partial class MerchantColumnPreference
{
    public int MerchantColumnPreferenceId { get; set; }

    public int Mid { get; set; }

    public string GridName { get; set; } = null!;

    public string SelectedColumns { get; set; } = null!;

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual Merchant MidNavigation { get; set; } = null!;
}
