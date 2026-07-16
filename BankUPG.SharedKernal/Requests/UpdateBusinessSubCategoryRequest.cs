using System.ComponentModel.DataAnnotations;

namespace BankUPG.SharedKernal.Requests
{
    public class UpdateBusinessSubCategoryRequest
    {
        [Required]
        public int BusinessSubCategoryId { get; set; }

        [Required]
        public int BusinessCategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string SubCategoryName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string SubCategoryCode { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public bool? IsActive { get; set; }
    }
}