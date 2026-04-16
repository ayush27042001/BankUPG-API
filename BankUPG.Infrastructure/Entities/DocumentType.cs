using System;
using System.Collections.Generic;

namespace BankUPG.Infrastructure.Entities;

public partial class DocumentType
{
    public int DocumentTypeId { get; set; }

    public string DocumentName { get; set; } = null!;

    public string DocumentCode { get; set; } = null!;

    public string? AllowedExtensions { get; set; }

    public int? MaxFileSizeMb { get; set; }

    public bool? IsRequired { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ICollection<DocumentUpload> DocumentUploads { get; set; } = new List<DocumentUpload>();
}
