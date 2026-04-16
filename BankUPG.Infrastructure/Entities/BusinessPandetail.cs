using System;
using System.Collections.Generic;

namespace BankUPG.Infrastructure.Entities;

public partial class BusinessPandetail
{
    public int BusinessPandetailId { get; set; }

    public int Mid { get; set; }

    public string PancardNumber { get; set; } = null!;

    public string NameOnPancard { get; set; } = null!;

    public DateOnly DateOfBirthOrIncorporation { get; set; }

    public string? PanverificationStatus { get; set; }

    public DateTime? PanverifiedDate { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual Merchant MidNavigation { get; set; } = null!;
}
