namespace BankUPG.SharedKernal.Responses
{
    public class DocumentTypeDetailResponse
    {
        public int DocumentTypeId { get; set; }
        public string DocumentName { get; set; } = string.Empty;
        public string DocumentCode { get; set; } = string.Empty;
        public string? AllowedExtensions { get; set; }
        public int? MaxFileSizeMb { get; set; }
        public bool? IsRequired { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    public class BusinessProofTypeDetailResponse
    {
        public int BusinessProofTypeId { get; set; }
        public string ProofName { get; set; } = string.Empty;
        public string ProofCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    public class DocumentTypeListResponse
    {
        public List<DocumentTypeDetailResponse> DocumentTypes { get; set; } = new();
        public int TotalCount { get; set; }
    }

    public class BusinessProofTypeListResponse
    {
        public List<BusinessProofTypeDetailResponse> BusinessProofTypes { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
