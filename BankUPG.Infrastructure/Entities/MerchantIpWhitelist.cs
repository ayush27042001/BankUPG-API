using System;

namespace BankUPG.Infrastructure.Entities;

public partial class MerchantIpWhitelist
{
    public int IpWhitelistId { get; set; }

    public int Mid { get; set; }

    public string IpAddress { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual Merchant MidNavigation { get; set; } = null!;
}
