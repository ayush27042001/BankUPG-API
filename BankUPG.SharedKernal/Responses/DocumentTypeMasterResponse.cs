namespace BankUPG.SharedKernal.Responses
{
    public class DocumentTypeMasterResponse
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
}