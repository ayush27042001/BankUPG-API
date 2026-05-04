namespace BankUPG.Infrastructure.Entities
{
    public class RefreshToken
    {
        public int RefreshTokenId { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRevoked { get; set; } = false;
        public DateTime? RevokedAt { get; set; }
        public string? ReplacedByToken { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsActive => !IsRevoked && !IsExpired;

        public virtual User User { get; set; } = null!;
    }
}
