using System;
using System.Collections.Generic;

namespace BankUPG.SharedKernal.Responses
{
    public class InvoiceResponse
    {
        public long InvoiceId { get; set; }
        public int Mid { get; set; }
        public string? InvoiceNumber { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerPhone { get; set; }
        public decimal SubTotal { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Status { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public string? Notes { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public List<InvoiceItemResponse>? Items { get; set; }
    }

    public class InvoiceItemResponse
    {
        public long InvoiceItemId { get; set; }
        public string? Description { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; set; }
    }
}
