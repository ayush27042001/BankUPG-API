using System;
using System.ComponentModel.DataAnnotations;

namespace BankUPG.SharedKernal.Requests
{
    public class CreateMerchantColumnPreferenceRequest
    {
        [Required]
        public int Mid { get; set; }

        [Required]
        [MaxLength(100)]
        public string GridName { get; set; } = null!;

        [Required]
        public string SelectedColumns { get; set; } = null!;
    }

    public class UpdateMerchantColumnPreferenceRequest : CreateMerchantColumnPreferenceRequest
    {
        [Required]
        public int MerchantColumnPreferenceId { get; set; }
    }
}
