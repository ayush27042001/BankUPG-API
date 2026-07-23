using System;
using System.Collections.Generic;

namespace BankUPG.SharedKernal.Responses
{
    public class BatchRefundResponse
    {
        public long BatchRefundId { get; set; }
        public int Mid { get; set; }
        public string? BatchReferenceId { get; set; }
        public int TotalItems { get; set; }
        public decimal TotalAmount { get; set; }
        public int ProcessedItems { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public List<BatchRefundItemResponse>? Items { get; set; }
    }

    public class BatchRefundItemResponse
    {
        public long BatchRefundItemId { get; set; }
        public long TransactionId { get; set; }
        public long? RefundId { get; set; }
        public decimal Amount { get; set; }
        public string? Status { get; set; }
        public string? FailureReason { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
