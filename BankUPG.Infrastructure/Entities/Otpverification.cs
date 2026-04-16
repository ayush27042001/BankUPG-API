using System;
using System.Collections.Generic;

namespace BankUPG.Infrastructure.Entities;

public partial class Otpverification
{
    public int OtpverificationId { get; set; }

    public int? Mid { get; set; }

    public int? UserId { get; set; }

    public string MobileNumber { get; set; } = null!;

    public string Otpcode { get; set; } = null!;

    public string Otppurpose { get; set; } = null!;

    public DateTime? OtpcreatedTime { get; set; }

    public DateTime OtpexpiryTime { get; set; }

    public bool? IsUsed { get; set; }

    public DateTime? UsedTime { get; set; }

    public string? Ipaddress { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual Merchant? MidNavigation { get; set; }

    public virtual User? User { get; set; }
}
