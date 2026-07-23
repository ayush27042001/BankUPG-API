using System;
using System.Collections.Generic;

namespace BankUPG.Infrastructure.Entities;

public partial class PaymentOrder
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

    public virtual Merchant MidNavigation { get; set; } = null!;

    public virtual ICollection<PaymentAttempt> PaymentAttempts { get; set; } = new List<PaymentAttempt>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
