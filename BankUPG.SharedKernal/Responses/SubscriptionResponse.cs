using System;
using System.Collections.Generic;

namespace BankUPG.SharedKernal.Responses
{
    public class SubscriptionPlanResponse
    {
        public int SubscriptionPlanId { get; set; }
        public int Mid { get; set; }
        public string? PlanName { get; set; }
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
        public string? Interval { get; set; }
        public int IntervalCount { get; set; }
        public int? TotalBillingCycles { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    public class SubscriptionResponse
    {
        public long SubscriptionId { get; set; }
        public int Mid { get; set; }
        public int PlanId { get; set; }
        public string? PlanName { get; set; }
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
    }
}
