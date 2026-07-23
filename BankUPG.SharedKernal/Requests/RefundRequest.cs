using System;
using System.ComponentModel.DataAnnotations;

namespace BankUPG.SharedKernal.Requests
{
    public class ListRefundsRequest
    {
        public string? Status { get; set; }
        public string? RefundType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class InitiateRefundRequest
    {
        [Required]
        public long TransactionId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        public string? RefundType { get; set; } = "full";
        public string? MerchantReferenceId { get; set; }
    }
}
