using System;

namespace BankUPG.SharedKernal.Requests
{
    public class ListTransactionsRequest
    {
        // Transaction identifiers / customer search
        public long? TransactionId { get; set; }
        public string? CustomerEmail { get; set; }
        public string? MerchantReferenceId { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerName { get; set; }
        public string? UpiReference { get; set; }
        public string? BankReference { get; set; }

        // Date quick-filters: today, yesterday, last1hour, last7days, last30days, custom
        public string? DateFilter { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // Other filters
        public string? PaymentMode { get; set; }
        public string? Source { get; set; }        // PaymentGateway, PaymentLink, PaymentHandle, Invoice, Events
        public string? Status { get; set; }        // Success, InProgress, UserCancelled, Failed, AutoRefund, UserDropped, Bounced, RefundSuccess, RefundFailed, RefundPending, Cancelled, Authorized
        public string? RefundType { get; set; }    // full, partial
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public long? OrderId { get; set; }
        public long? PaymentLinkId { get; set; }
        public long? SubscriptionId { get; set; }
        public long? InvoiceId { get; set; }

        public string? SortBy { get; set; }
        public string? SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
