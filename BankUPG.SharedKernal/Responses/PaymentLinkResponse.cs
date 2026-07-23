using System;

namespace BankUPG.SharedKernal.Responses
{
    public class PaymentLinkResponse
    {
        // User-selected columns
        public DateTime? CreatedOn => CreatedDate;
        public long PaymentLinkId { get; set; }
        public string? PurposeOfPayment => Purpose ?? Description;
        public string? InvoiceID => InvoiceId;
        public decimal? Amount { get; set; }
        public string? PaymentLink => ShortUrl;
        public string? PaymentType { get; set; }
        public string? Status { get; set; }

        // Available columns
        public int Mid { get; set; }
        public string? Description { get; set; }
        public string? Purpose { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerMobile => CustomerPhone;
        public string? CustomerPhone { get; set; }
        public string? CustomerName { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string? ShortUrl { get; set; }
        public string? ReferenceId { get; set; }
        public string? InvoiceId { get; set; }
        public string? AmountType { get; set; }
        public bool? IsPartialPayment { get; set; }
        public int? MaxPaymentsAllowed { get; set; }
        public int? MaxUses { get; set; }
        public int UsedCount { get; set; }
        public int TotalViews { get; set; }
        public decimal TotalAmountPaid { get; set; }
        public bool? SendSms { get; set; }
        public List<PaymentLinkDataCaptureFieldResponse>? CustomerDataCapture { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    public class PaymentLinkDataCaptureFieldResponse
    {
        public string? FieldType { get; set; }
        public string? Name { get; set; }
        public string? Options { get; set; }
    }

    public class PaymentLinkSummaryResponse
    {
        public int ActivePaymentLinks { get; set; }
        public decimal RevenueViaPaymentLinks { get; set; }
        public int TotalViews { get; set; }
        public int TotalLinks { get; set; }
        public int PaidLinks { get; set; }
    }
}
