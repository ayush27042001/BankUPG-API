using System;
using System.ComponentModel.DataAnnotations;

namespace BankUPG.SharedKernal.Requests
{
    public class CreatePaymentOrderRequest
    {
        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        public string? OrderRef { get; set; }
        public string? Currency { get; set; } = "INR";
        public string? CustomerEmail { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerName { get; set; }
        public string? Notes { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

    public class ListPaymentOrdersRequest
    {
        public string? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
