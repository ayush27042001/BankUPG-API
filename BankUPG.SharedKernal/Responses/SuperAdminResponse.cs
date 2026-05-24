namespace BankUPG.SharedKernal.Responses
{
    public class AdminVerifyOtpData
    {
        public string Token { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public DateTime? Expiration { get; set; }
        public DateTime? RefreshTokenExpiration { get; set; }
        public string? Username { get; set; }
        public string? Role { get; set; }
    }

    public class AdminRefreshTokenResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTime? Expiration { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiration { get; set; }
        public string? Username { get; set; }
        public string? Role { get; set; }
    }
}
