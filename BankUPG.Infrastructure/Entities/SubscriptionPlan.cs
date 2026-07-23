using System;
using System.Collections.Generic;

namespace BankUPG.Infrastructure.Entities;

public partial class SubscriptionPlan
{
    public int SubscriptionPlanId { get; set; }

    public int Mid { get; set; }

    public string PlanName { get; set; } = null!;

    public decimal Amount { get; set; }

    public string? Currency { get; set; }

    public string? Interval { get; set; }

    public int IntervalCount { get; set; }

    public int? TotalBillingCycles { get; set; }

    public bool IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual Merchant MidNavigation { get; set; } = null!;

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
