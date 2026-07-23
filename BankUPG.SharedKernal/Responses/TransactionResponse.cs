using System;
using System.Collections.Generic;

namespace BankUPG.SharedKernal.Responses
{
    public class TransactionResponse
    {
        // Selected columns (default shown)
        public DateTime? Date => TransactionDate ?? CreatedDate;
        public long TransactionId { get; set; }
        public string? PaymentMode { get; set; }
        public string? Email => CustomerEmail;
        public decimal? Amount { get; set; }
        public string? Status { get; set; }
        public string? MerchantReferenceId { get; set; }
        public string? RefundType { get; set; }        // full / partial / null
        public string? Source { get; set; }            // PaymentGateway, PaymentLink, PaymentHandle, Invoice, Events
        public string? BankRRN => BankReference;        // Bank Reference Number

        // Available extra columns
        public int Mid { get; set; }
        public string? PayuId { get; set; }
        public string? PaymentAggregator { get; set; }
        public string? BankARN => BankReference;        // Bank ARN
        public string? CustomerEmail { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerName { get; set; }
        public string? UpiReference { get; set; }
        public string? BankReference { get; set; }
        public DateTime? TransactionDate { get; set; }
        public long? OrderId { get; set; }
        public long? PaymentLinkId { get; set; }
        public long? SubscriptionId { get; set; }
        public long? InvoiceId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public TransactionChargeSummary? ChargeSummary { get; set; }
        public int RefundCount { get; set; }
        public decimal? RefundedAmount { get; set; }
    }

    public class TransactionSummaryResponse
    {
        public decimal TotalPayments { get; set; }        // Sum of transaction amounts in filtered range
        public int NumberOfTransactions { get; set; }     // Total count
        public int SuccessCount { get; set; }
        public decimal SuccessRate { get; set; }          // percentage (0-100)
        public int FailedCount { get; set; }
        public int PendingCount { get; set; }
        public decimal RefundedAmount { get; set; }
    }

    public class TransactionChargeSummary
    {
        public string? PaymentMethodType { get; set; }
        public string? ChargeType { get; set; }
        public decimal ChargeValue { get; set; }
        public decimal MdrAmount { get; set; }
        public decimal GstAmount { get; set; }
        public decimal TotalDeduction { get; set; }
        public decimal NetAmount { get; set; }
    }

    public class TransactionChargeDetailResponse
    {
        public long TransactionChargeId { get; set; }
        public long TransactionId { get; set; }
        public int Mid { get; set; }
        public string? PaymentMethodType { get; set; }
        public string? NetworkName { get; set; }
        public string? ChargeType { get; set; }
        public decimal ChargeValue { get; set; }
        public decimal TransactionAmount { get; set; }
        public decimal ChargeAmount { get; set; }
        public decimal GstAmount { get; set; }
        public decimal TotalDeduction { get; set; }
        public decimal NetAmount { get; set; }
        public DateTime? CreatedDate { get; set; }
    }

    public class PaymentMethodChargeResponse
    {
        public int PaymentMethodChargeId { get; set; }
        public string? PaymentMethodType { get; set; }
        public string? NetworkName { get; set; }
        public string? ChargeType { get; set; }
        public decimal ChargeValue { get; set; }
        public decimal? MinCharge { get; set; }
        public decimal? MaxCharge { get; set; }
        public decimal GstPercentage { get; set; }
        public bool IsActive { get; set; }
    }
}
