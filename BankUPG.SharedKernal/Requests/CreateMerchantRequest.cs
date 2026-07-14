using System.ComponentModel.DataAnnotations;

namespace BankUPG.SharedKernal.Requests
{
    public class CreateMerchantRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(200)]
        public string BusinessName { get; set; } = string.Empty;

        [Required]
        public int BusinessEntityTypeId { get; set; }

        [Required]
        public int BusinessCategoryId { get; set; }

        [Required]
        public int BusinessSubCategoryId { get; set; }

        public decimal? ExpectedSalesPerMonth { get; set; }

        public bool? HasGstin { get; set; }

        [StringLength(20)]
        public string? Gstin { get; set; }

        public bool? CkycconsentGiven { get; set; }

        public string? Ckycidentifier { get; set; }
    }
}