using System;
using System.ComponentModel.DataAnnotations;

namespace BankUPG.SharedKernal.Requests
{
    public class CreatePaymentLinkRequest
    {
        [MaxLength(500)]
        public string? Description { get; set; }     // Purpose of payment

        public decimal? Amount { get; set; }

        public string? AmountType { get; set; } = "fixed";

        public DateTime? ExpiryDate { get; set; }
        public DateTime? DueDate { get; set; }

        // Automatic expiry: use validation period if ExpiryDate is not set
        public int? ValidationPeriod { get; set; }
        public string? TimeUnit { get; set; }       // D, H, M

        public string? PaymentType { get; set; } = "Standard";  // Standard, PartialPayment

        public bool? IsPartialPayment { get; set; }
        public int? MaxPaymentsAllowed { get; set; }

        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerEmail { get; set; }

        public string? ReferenceId { get; set; }     // Merchant Reference ID
        public string? InvoiceId { get; set; }

        public int? MaxUses { get; set; }
        public bool? SendSms { get; set; }

        public List<PaymentLinkDataCaptureFieldRequest>? CustomerDataCapture { get; set; }
    }

    public class PaymentLinkDataCaptureFieldRequest
    {
        [Required]
        public string FieldType { get; set; } = null!; // Text, Number, Email, Date, Dropdown

        [Required]
        public string Name { get; set; } = null!;
        public string? Options { get; set; }          // comma separated for Dropdown
    }

    public class ListPaymentLinksRequest
    {
        public string? DateFilter { get; set; }      // today, yesterday, last7days, last30days, custom
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Status { get; set; }          // active, deactivated, expired, paid, cancelled, created
        public string? PaymentType { get; set; }     // Standard, PartialPayment
        public string? Purpose { get; set; }
        public string? InvoiceId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerPhone { get; set; }
        public string? ReferenceId { get; set; }

        public string? SortBy { get; set; }
        public string? SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
