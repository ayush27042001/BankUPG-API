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
    }
}
