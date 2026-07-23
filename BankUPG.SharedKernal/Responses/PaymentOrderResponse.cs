using System;
using System.Collections.Generic;

namespace BankUPG.SharedKernal.Responses
{
    public class PaymentOrderResponse
    {
        public long PaymentOrderId { get; set; }
        public int Mid { get; set; }
        public string? OrderRef { get; set; }
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerName { get; set; }
        public string? Notes { get; set; }
        public string? Status { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public List<PaymentAttemptResponse>? Attempts { get; set; }
    }

    public class PaymentAttemptResponse
    {
        public long PaymentAttemptId { get; set; }
        public long OrderId { get; set; }
        public long? TransactionId { get; set; }
        public string? PaymentMode { get; set; }
        public decimal Amount { get; set; }
        public string? Status { get; set; }
        public string? FailureReason { get; set; }
        public DateTime? AttemptDate { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
