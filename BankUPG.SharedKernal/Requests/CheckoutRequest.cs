using System.ComponentModel.DataAnnotations;

namespace BankUPG.SharedKernal.Requests
{
    public class CheckoutInitiateRequest
    {
        [Required]
        [Range(1, 10000000, ErrorMessage = "Amount must be between 1 and 1,00,00,000")]
        public decimal Amount { get; set; }

        public string Currency { get; set; } = "INR";

        [Required]
        [MaxLength(200)]
        public string OrderRef { get; set; } = null!;

        [MaxLength(200)]
        public string? CustomerName { get; set; }

        [EmailAddress]
        [MaxLength(200)]
        public string? CustomerEmail { get; set; }

        [MaxLength(15)]
        public string? CustomerPhone { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        [MaxLength(1000)]
        public string? CallbackUrl { get; set; }

        [MaxLength(1000)]
        public string? CancelUrl { get; set; }
    }

    public class CheckoutPayCardRequest
    {
        [Required]
        public string CheckoutToken { get; set; } = null!;

        public string PaymentMode { get; set; } = "Card";

        [MaxLength(19)]
        public string? CardNumber { get; set; }

        [MaxLength(100)]
        public string? CardName { get; set; }

        [MaxLength(7)]
        public string? CardExpiry { get; set; }

        [MaxLength(4)]
        public string? CardCvv { get; set; }

        [MaxLength(100)]
        public string? UpiVpa { get; set; }

        [MaxLength(50)]
        public string? BankCode { get; set; }
    }

    public class CheckoutVerifyRequest
    {
        [Required]
        public string BankupgPaymentId { get; set; } = null!;

        [Required]
        public string BankupgOrderId { get; set; } = null!;

        [Required]
        public string BankupgSignature { get; set; } = null!;
    }
}
