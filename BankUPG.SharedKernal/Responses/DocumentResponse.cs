namespace BankUPG.SharedKernal.Responses
{
    public class DocumentResponse
    {
        public int DocumentUploadId { get; set; }
        public int Mid { get; set; }
        public int DocumentTypeId { get; set; }
        public string DocumentTypeName { get; set; } = string.Empty;
        public string DocumentTypeCode { get; set; } = string.Empty;
        public int? BusinessProofTypeId { get; set; }
        public string? BusinessProofTypeName { get; set; }
        public string DocumentFileName { get; set; } = string.Empty;
        public string DocumentFilePath { get; set; } = string.Empty;
        public long? DocumentSizeBytes { get; set; }
        public string? DocumentMimeType { get; set; }
        public bool? IsVerified { get; set; }
        public int? VerifiedBy { get; set; }
        public DateTime? VerifiedDate { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    public class DocumentUploadResponse
    {
        public int DocumentUploadId { get; set; }
        public string DocumentFileName { get; set; } = string.Empty;
        public string DocumentFilePath { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public OnboardingStatusDto? OnboardingStatus { get; set; }
    }

    public class DocumentListResponse
    {
        public List<DocumentResponse> Documents { get; set; } = new();
        public int TotalCount { get; set; }
    }

    public class DocumentTypeResponse
    {
        public int DocumentTypeId { get; set; }
        public string DocumentName { get; set; } = string.Empty;
        public string DocumentCode { get; set; } = string.Empty;
        public string? AllowedExtensions { get; set; }
        public int? MaxFileSizeMb { get; set; }
        public bool? IsRequired { get; set; }
        public bool? IsActive { get; set; }
    }

    public class BusinessProofTypeResponse
    {
        public int BusinessProofTypeId { get; set; }
        public string ProofName { get; set; } = string.Empty;
        public string ProofCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
    }
}
