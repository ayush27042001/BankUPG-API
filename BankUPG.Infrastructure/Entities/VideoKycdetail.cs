using System;
using System.Collections.Generic;

namespace BankUPG.Infrastructure.Entities;

public partial class VideoKycdetail
{
    public int VideoKycdetailId { get; set; }

    public int Mid { get; set; }

    public string? VideoKycstatus { get; set; }

    public DateTime? ScheduledDateTime { get; set; }

    public DateTime? CompletedDateTime { get; set; }

    public int? AgentId { get; set; }

    public string? AgentName { get; set; }

    public bool? AadhaarVerified { get; set; }

    public string? DigilockerReferenceNumber { get; set; }

    public string? VideoRecordingUrl { get; set; }

    public string? Remarks { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual Merchant MidNavigation { get; set; } = null!;
}
