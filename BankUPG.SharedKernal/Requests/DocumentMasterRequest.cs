using System.ComponentModel.DataAnnotations;

namespace BankUPG.SharedKernal.Requests
{
    public class CreateDocumentTypeRequest
    {
        [Required(ErrorMessage = "Document name is required")]
        [MaxLength(100, ErrorMessage = "Document name cannot exceed 100 characters")]
        public string DocumentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Document code is required")]
        [MaxLength(50, ErrorMessage = "Document code cannot exceed 50 characters")]
        public string DocumentCode { get; set; } = string.Empty;

        [MaxLength(200, ErrorMessage = "Allowed extensions cannot exceed 200 characters")]
        public string? AllowedExtensions { get; set; }

        [Range(1, 100, ErrorMessage = "Max file size must be between 1 and 100 MB")]
        public int? MaxFileSizeMb { get; set; }

        public bool? IsRequired { get; set; }
    }

    public class UpdateDocumentTypeRequest
    {
        [Required(ErrorMessage = "Document type ID is required")]
        public int DocumentTypeId { get; set; }

        [Required(ErrorMessage = "Document name is required")]
        [MaxLength(100, ErrorMessage = "Document name cannot exceed 100 characters")]
        public string DocumentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Document code is required")]
        [MaxLength(50, ErrorMessage = "Document code cannot exceed 50 characters")]
        public string DocumentCode { get; set; } = string.Empty;

        [MaxLength(200, ErrorMessage = "Allowed extensions cannot exceed 200 characters")]
        public string? AllowedExtensions { get; set; }

        [Range(1, 100, ErrorMessage = "Max file size must be between 1 and 100 MB")]
        public int? MaxFileSizeMb { get; set; }

        public bool? IsRequired { get; set; }

        public bool? IsActive { get; set; }
    }

    public class CreateBusinessProofTypeRequest
    {
        [Required(ErrorMessage = "Proof name is required")]
        [MaxLength(100, ErrorMessage = "Proof name cannot exceed 100 characters")]
        public string ProofName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Proof code is required")]
        [MaxLength(50, ErrorMessage = "Proof code cannot exceed 50 characters")]
        public string ProofCode { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }
    }

    public class UpdateBusinessProofTypeRequest
    {
        [Required(ErrorMessage = "Business proof type ID is required")]
        public int BusinessProofTypeId { get; set; }

        [Required(ErrorMessage = "Proof name is required")]
        [MaxLength(100, ErrorMessage = "Proof name cannot exceed 100 characters")]
        public string ProofName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Proof code is required")]
        [MaxLength(50, ErrorMessage = "Proof code cannot exceed 50 characters")]
        public string ProofCode { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        public bool? IsActive { get; set; }
    }
}
