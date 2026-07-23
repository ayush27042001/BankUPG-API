using System;
using System.ComponentModel.DataAnnotations;

namespace BankUPG.SharedKernal.Requests
{
    public class CreatePaymentMethodChargeRequest
    {
        [MaxLength(50)]
        public string? PaymentMethodType { get; set; }

        [MaxLength(50)]
        public string? NetworkName { get; set; }

        [MaxLength(50)]
        public string? ChargeType { get; set; } // Percentage, Fixed

        [Range(0, 100)]
        public decimal ChargeValue { get; set; }

        public decimal? MinCharge { get; set; }
        public decimal? MaxCharge { get; set; }

        [Range(0, 100)]
        public decimal GstPercentage { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdatePaymentMethodChargeRequest : CreatePaymentMethodChargeRequest
    {
        [Required]
        public int PaymentMethodChargeId { get; set; }
    }
}
