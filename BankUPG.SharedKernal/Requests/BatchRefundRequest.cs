using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BankUPG.SharedKernal.Requests
{
    public class CreateBatchRefundRequest
    {
        public string? BatchReferenceId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one refund item is required")]
        public List<BatchRefundItemRequest> Items { get; set; } = new();
    }

    public class BatchRefundItemRequest
    {
        [Required]
        public long TransactionId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }
    }

    public class ListBatchRefundsRequest
    {
        public string? Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
