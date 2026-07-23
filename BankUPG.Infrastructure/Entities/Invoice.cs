using System;
using System.Collections.Generic;

namespace BankUPG.Infrastructure.Entities;

public partial class Invoice
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

    public virtual Merchant MidNavigation { get; set; } = null!;

    public virtual ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
