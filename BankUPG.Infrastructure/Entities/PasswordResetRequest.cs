using System;
using System.Collections.Generic;

namespace BankUPG.Infrastructure.Entities;

public partial class PasswordResetRequest
{
    public int PasswordResetRequestId { get; set; }

    public int UserId { get; set; }

    public string ResetToken { get; set; } = null!;

    public DateTime TokenExpiryTime { get; set; }

    public bool? IsUsed { get; set; }

    public DateTime? UsedTime { get; set; }

    public string? Ipaddress { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual User User { get; set; } = null!;
}
