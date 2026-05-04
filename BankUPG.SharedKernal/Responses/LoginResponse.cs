namespace BankUPG.SharedKernal.Responses
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? MobileNumber { get; set; }
        public bool IsMobileVerified { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        
        // Refresh token for token renewal
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiration { get; set; }
        
        // Onboarding status - to redirect user to correct step
        public int? OnboardingStatusId { get; set; }
        public string? CurrentStepName { get; set; }
        public string? FormStep { get; set; }
        public int? Step { get; set; }
        public OnboardingStatusDto? OnboardingStatus { get; set; }
    }

    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class RefreshTokenResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiration { get; set; }
        public string Email { get; set; } = string.Empty;
    }
}
