using System;
using System.Collections.Generic;

namespace BankUPG.Infrastructure.Entities;

public partial class WebsiteAppDetail
{
    public int WebsiteAppDetailId { get; set; }

    public int Mid { get; set; }

    public string PaymentCollectionPreference { get; set; } = null!;

    public string? WebsiteAppUrl { get; set; }

    public string? AndroidAppUrl { get; set; }

    public string? IOsappUrl { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual Merchant MidNavigation { get; set; } = null!;
}
