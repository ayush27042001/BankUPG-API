namespace BankUPG.Infrastructure.Entities;

public class SuperAdmin
{
    public int AdminId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Salt { get; set; } = null!;

    public string Role { get; set; } = "SuperAdmin";

    public bool IsActive { get; set; } = true;

    public bool IsLocked { get; set; } = false;

    public int FailedLoginAttempts { get; set; } = 0;

    public DateTime? LastLoginDate { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ICollection<SuperAdminRefreshToken> RefreshTokens { get; set; } = new List<SuperAdminRefreshToken>();
}
