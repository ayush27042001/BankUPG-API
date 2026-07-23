using System;

namespace BankUPG.Infrastructure.Entities;

public partial class PaymentLinkBulkUploadFile
{
    public long FileId { get; set; }

    public int Mid { get; set; }

    public string FileName { get; set; } = null!;

    public string? FilePath { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual Merchant MidNavigation { get; set; } = null!;
}
