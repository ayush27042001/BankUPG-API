namespace BankUPG.SharedKernal.Responses
{
    public class RegistrationInitiatedResponse
    {
        public string RegistrationToken { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public int OtpExpirySeconds { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class RegistrationCompletedResponse
    {
        public int UserId { get; set; }
        public int Mid { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime TokenExpiration { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? FormStep { get; set; }
        public int? Step { get; set; }
        public OnboardingStatusDto OnboardingStatus { get; set; } = new();
    }

    public class OnboardingStatusDto
    {
        public int StepNumber { get; set; }
        public string StepName { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public List<OnboardingStepDto> Steps { get; set; } = new();
        public ConnectPlatformStepsDto? ConnectPlatformSteps { get; set; }
    }

    public class ConnectPlatformStepsDto
    {
        public int CurrentStep { get; set; }
        public int TotalSteps { get; set; } = 5;
        public List<ConnectPlatformStepDto> Steps { get; set; } = new();
    }

    public class ConnectPlatformStepDto
    {
        public int StepNumber { get; set; }
        public string StepName { get; set; } = string.Empty;
        public string StepKey { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public bool IsActive { get; set; }
    }

    public class OnboardingStepDto
    {
        public int StepNumber { get; set; }
        public string StepName { get; set; } = string.Empty;
        public string StepKey { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public bool IsActive { get; set; }
    }

    public class OtpVerificationResponse
    {
        public bool IsVerified { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? RegistrationToken { get; set; }
        public int? RemainingAttempts { get; set; }
        
        // User details returned on successful verification (before user creation)
        public int? UserId { get; set; }
        public int? Mid { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? MobileNumber { get; set; }
        public string? CompanyWebsite { get; set; }
        public string? Token { get; set; }
        public DateTime? TokenExpiration { get; set; }
        
        // Form flow tracking
        public string? FormStep { get; set; }
        public int Step { get; set; }
    }

    public class ResendOtpResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int RemainingSeconds { get; set; }
        public int MaxResendAttempts { get; set; }
        public int RemainingResendAttempts { get; set; }
    }

    public class PanDetailsResponse
    {
        public string PanCardNumber { get; set; } = string.Empty;
        public string NameOnPanCard { get; set; } = string.Empty;
        public DateTime DateOfBirthOrIncorporation { get; set; }
        public string? VerificationStatus { get; set; }
        public int? MerchantId { get; set; }
    }
}
