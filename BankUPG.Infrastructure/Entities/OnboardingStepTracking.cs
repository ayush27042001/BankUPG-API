using System;
using System.Collections.Generic;

namespace BankUPG.Infrastructure.Entities;

public partial class OnboardingStepTracking
{
    public int OnboardingStepTrackingId { get; set; }
    public int Mid { get; set; }
    public string StepName { get; set; } = null!;
    public string StepStatus { get; set; } = null!;
    public DateTime? StartedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public DateTime? CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public string? StepKey { get; set; }
    public bool? IsCompleted { get; set; }
    public virtual Merchant MidNavigation { get; set; } = null!;
}
