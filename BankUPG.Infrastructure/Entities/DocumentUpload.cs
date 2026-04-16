using System;
using System.Collections.Generic;

namespace BankUPG.Infrastructure.Entities;

public partial class DocumentUpload
{
    public int DocumentUploadId { get; set; }

    public int Mid { get; set; }

    public int DocumentTypeId { get; set; }

    public int? BusinessProofTypeId { get; set; }

    public string DocumentFileName { get; set; } = null!;

    public string DocumentFilePath { get; set; } = null!;

    public long? DocumentSizeBytes { get; set; }

    public string? DocumentMimeType { get; set; }

    public bool? IsVerified { get; set; }

    public int? VerifiedBy { get; set; }

    public DateTime? VerifiedDate { get; set; }

    public string? RejectionReason { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual BusinessProofType? BusinessProofType { get; set; }

    public virtual DocumentType DocumentType { get; set; } = null!;

    public virtual Merchant MidNavigation { get; set; } = null!;
}
