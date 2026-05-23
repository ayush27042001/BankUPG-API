namespace BankUPG.SharedKernal.Responses
{
    public class StatusTrackerStepDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "pending";
        public string Icon { get; set; } = string.Empty;
        public string? CompletedDate { get; set; }
        public string? Remarks { get; set; }
    }

    public class StatusTrackerResponse
    {
        public string ApplicationId { get; set; } = string.Empty;
        public List<StatusTrackerStepDto> StatusSteps { get; set; } = new();
        public int OverallProgress { get; set; }
        public string LastUpdated { get; set; } = string.Empty;
        public bool IsOnboardingCompleted { get; set; }
        public bool IsOnboardingRejected { get; set; }
    }
}
