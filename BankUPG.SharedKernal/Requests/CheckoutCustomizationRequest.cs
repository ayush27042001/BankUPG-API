using System;
using System.ComponentModel.DataAnnotations;

namespace BankUPG.SharedKernal.Requests
{
    public class CreateCheckoutCustomizationRequest
    {
        [Required]
        public int Mid { get; set; }

        [MaxLength(1000)]
        public string? BrandLogoUrl { get; set; }

        [MaxLength(10)]
        public string? PrimaryColor { get; set; }

        [MaxLength(10)]
        public string? SecondaryColor { get; set; }

        [MaxLength(10)]
        public string? Language { get; set; }

        [MaxLength(1000)]
        public string? OwnerSignatureUrl { get; set; }
    }

    public class UpdateCheckoutCustomizationRequest : CreateCheckoutCustomizationRequest
    {
        [Required]
        public int CheckoutCustomizationId { get; set; }
    }
}
