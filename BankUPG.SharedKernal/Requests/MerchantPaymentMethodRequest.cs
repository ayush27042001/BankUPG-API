using System;
using System.ComponentModel.DataAnnotations;

namespace BankUPG.SharedKernal.Requests
{
    public class CreateMerchantPaymentMethodRequest
    {
        [Required]
        public int Mid { get; set; }

        [MaxLength(50)]
        public string? PaymentMethodType { get; set; }

        [MaxLength(100)]
        public string? SubMethodName { get; set; }

        public bool? IsActive { get; set; } = true;
    }

    public class UpdateMerchantPaymentMethodRequest : CreateMerchantPaymentMethodRequest
    {
        [Required]
        public int MerchantPaymentMethodId { get; set; }
    }
}
