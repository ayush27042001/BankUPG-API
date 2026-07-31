using System;

namespace BankUPG.SharedKernal.Responses
{
    public class CheckoutOrderResponse
    {
        public string OrderId { get; set; } = null!;
        public string CheckoutToken { get; set; } = null!;
        public string CheckoutUrl { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public string OrderRef { get; set; } = null!;
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerPhone { get; set; }
        public string Status { get; set; } = "created";
        public DateTime ExpiryDate { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class CheckoutSessionResponse
    {
        public string OrderId { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public string OrderRef { get; set; } = null!;
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerPhone { get; set; }
        public string Status { get; set; } = "created";
        public DateTime ExpiryDate { get; set; }
        public string? MerchantName { get; set; }
        public string? MerchantLogoUrl { get; set; }
        public string? PrimaryColor { get; set; }
        public string? SecondaryColor { get; set; }
        public string? Language { get; set; }
        public bool IsExpired { get; set; }
        public bool IsPaid { get; set; }
        public List<string> EnabledPaymentModes { get; set; } = new List<string>();
    }

    public class CheckoutPayResponse
    {
        public bool Success { get; set; }
        public string? PaymentId { get; set; }
        public string? OrderId { get; set; }
        public string? Status { get; set; }
        public string? Message { get; set; }
        public string? RedirectUrl { get; set; }
        public string? Signature { get; set; }
        public decimal Amount { get; set; }
        public string? PaymentMode { get; set; }
        public DateTime? PaidAt { get; set; }
    }

    public class CheckoutVerifyResponse
    {
        public bool IsValid { get; set; }
        public string? PaymentId { get; set; }
        public string? OrderId { get; set; }
        public string? Status { get; set; }
        public decimal Amount { get; set; }
        public string? PaymentMode { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? Message { get; set; }
    }

    public class CheckoutStatusResponse
    {
        public string OrderId { get; set; } = null!;
        public string OrderRef { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public string Status { get; set; } = null!;
        public string? PaymentId { get; set; }
        public string? PaymentMode { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}
