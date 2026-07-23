using System;
using System.ComponentModel.DataAnnotations;

namespace BankUPG.SharedKernal.Requests
{
    public class CreateMerchantApiKeyRequest
    {
        [Required]
        public int Mid { get; set; }

        [MaxLength(255)]
        public string? ApiKey { get; set; }

        [MaxLength(255)]
        public string? ApiSalt { get; set; }

        [MaxLength(255)]
        public string? ClientId { get; set; }

        [MaxLength(255)]
        public string? ClientSecretHash { get; set; }
    }

    public class UpdateMerchantApiKeyRequest : CreateMerchantApiKeyRequest
    {
        [Required]
        public int MerchantApiKeyId { get; set; }
    }
}
