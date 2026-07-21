namespace BankUPG.SharedKernal.Responses
{
    public class UserDocumentResponse
    {
        public string? MerchantDetail { get; set; }
        public string? DocumentName { get; set; }
        public bool? IsRequired { get; set; }
        public int? MaxFileSizeMB { get; set; }
        public string? AllowedExtensions { get; set; }
        public string? DocumentFileName { get; set; }
        public long? DocumentSizeBytes { get; set; }
        public bool? IsVerified { get; set; }
        public string? RejectionReason { get; set; }
        public int? VerifiedBy { get; set; }
        public DateTime? VerifiedDate { get; set; }
        public string? DocumentFilePath { get; set; }
        public string? DocumentMimeType { get; set; }
        public int? DocumentUploadID { get; set; }
        public string? ProofName { get; set; }
    }
}
