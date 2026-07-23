using System.ComponentModel.DataAnnotations;

namespace BankUPG.SharedKernal.Requests
{
    public class CreateSubscriptionPlanRequest
    {
        [Required]
        [MaxLength(200)]
        public string PlanName { get; set; } = null!;

        [Required]
        [Range(1, double.MaxValue)]
        public decimal Amount { get; set; }

        public string? Currency { get; set; } = "INR";

        [Required]
        public string Interval { get; set; } = null!;

        public int IntervalCount { get; set; } = 1;
        public int? TotalBillingCycles { get; set; }
    }

    public class UpdateSubscriptionPlanRequest
    {
        [MaxLength(200)]
        public string? PlanName { get; set; }

        public bool? IsActive { get; set; }
    }

    public class CreateSubscriptionRequest
    {
        [Required]
        public int PlanId { get; set; }

        public string? CustomerEmail { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerName { get; set; }
        public int? TotalCycles { get; set; }
    }

    public class ListSubscriptionsRequest
    {
        public string? Status { get; set; }
        public int? PlanId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
