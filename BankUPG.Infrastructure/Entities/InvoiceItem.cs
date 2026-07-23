using System;

namespace BankUPG.Infrastructure.Entities;

public partial class InvoiceItem
{
    public long InvoiceItemId { get; set; }

    public long InvoiceId { get; set; }

    public string? Description { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal Amount { get; set; }

    public virtual Invoice Invoice { get; set; } = null!;
}
