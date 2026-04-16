using System;
using System.Collections.Generic;

namespace BankUPG.Infrastructure.Entities;

public partial class User
{
    public int UserId { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Salt { get; set; } = null!;

    public string? MobileNumber { get; set; }

    public bool? IsEmailVerified { get; set; }

    public bool? IsMobileVerified { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsLocked { get; set; }

    public int? FailedLoginAttempts { get; set; }

    public DateTime? LastLoginDate { get; set; }

    public DateTime? PasswordLastChangedDate { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ICollection<LoginAuditLog> LoginAuditLogs { get; set; } = new List<LoginAuditLog>();

    public virtual ICollection<Merchant> Merchants { get; set; } = new List<Merchant>();

    public virtual ICollection<Otpverification> Otpverifications { get; set; } = new List<Otpverification>();

    public virtual ICollection<PasswordResetRequest> PasswordResetRequests { get; set; } = new List<PasswordResetRequest>();
}
