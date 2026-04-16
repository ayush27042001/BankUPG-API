using System;
using System.Collections.Generic;

namespace BankUPG.Infrastructure.Entities;

public partial class OnboardingStatus
{
    public int OnboardingStatusId { get; set; }

    public string StatusName { get; set; } = null!;

    public string? StatusDescription { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ICollection<Merchant> Merchants { get; set; } = new List<Merchant>();
}
