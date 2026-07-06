using System.ComponentModel.DataAnnotations;

namespace BankUPG.SharedKernal.Requests
{
    public class CreateBusinessProofTypeMasterRequest
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

    public class GetBusinessProofTypeListRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
    }

    public class UpdateBusinessProofTypeMasterRequest
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
