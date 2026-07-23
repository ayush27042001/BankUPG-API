using System;
using System.Collections.Generic;

namespace BankUPG.Infrastructure.Entities;

public partial class Subscription
{
    public long SubscriptionId { get; set; }

    public int Mid { get; set; }

    public int PlanId { get; set; }

    public string? CustomerEmail { get; set; }

    public string? CustomerPhone { get; set; }

    public string? CustomerName { get; set; }

    public string? Status { get; set; }

    public int CurrentCycle { get; set; }

    public int? TotalCycles { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime? NextBillingDate { get; set; }

    public string? UpiMandateRef { get; set; }

    public string? NachMandateRef { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual Merchant MidNavigation { get; set; } = null!;

    public virtual SubscriptionPlan Plan { get; set; } = null!;

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
