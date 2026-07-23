using System;

namespace BankUPG.Infrastructure.Entities;

public partial class PaymentLinkBulkUpload
{
    public long BulkUploadId { get; set; }

    public int Mid { get; set; }

    public string? BatchReferenceId { get; set; }

    public string? CreatorEmail { get; set; }

    public string? BatchDescription { get; set; }

    public string? FileName { get; set; }

    public int LinkCreated { get; set; }

    public int ActiveCount { get; set; }

    public string? Status { get; set; }

    public bool SendEmail { get; set; }

    public bool SendSms { get; set; }

    public string? CustomerDataCapture { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual Merchant MidNavigation { get; set; } = null!;
}
