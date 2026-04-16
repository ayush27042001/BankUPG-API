using System;
using System.Collections.Generic;

namespace BankUPG.Infrastructure.Entities;

public partial class LoginAuditLog
{
    public int LoginAuditLogId { get; set; }

    public int UserId { get; set; }

    public int? Mid { get; set; }

    public string LoginStatus { get; set; } = null!;

    public string? LoginIpaddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTime? LoginAttemptDate { get; set; }

    public virtual Merchant? MidNavigation { get; set; }

    public virtual User User { get; set; } = null!;
}
