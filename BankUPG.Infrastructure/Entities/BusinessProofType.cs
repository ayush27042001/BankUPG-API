using System;
using System.Collections.Generic;

namespace BankUPG.Infrastructure.Entities;

public partial class BusinessProofType
{
    public int BusinessProofTypeId { get; set; }

    public string ProofName { get; set; } = null!;

    public string ProofCode { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ICollection<DocumentUpload> DocumentUploads { get; set; } = new List<DocumentUpload>();
}
