using System.ComponentModel.DataAnnotations;

namespace BankUPG.SharedKernal.Requests
{
    public class UpdateDocumentTypeMasterRequest
    {
        [Required]
        public int DocumentTypeId { get; set; }

        [Required]
        [StringLength(100)]
        public string DocumentName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string DocumentCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string AllowedExtensions { get; set; } = string.Empty;

        [Required]
        public int? MaxFileSizeMb { get; set; }

        public bool? IsRequired { get; set; }

        public bool? IsActive { get; set; }
    }
}