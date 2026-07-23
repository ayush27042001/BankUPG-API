using System;
using System.Collections.Generic;

namespace BankUPG.Infrastructure.Entities;

public partial class PaymentLink
{
    public long PaymentLinkId { get; set; }

    public int Mid { get; set; }

    public decimal? Amount { get; set; }

    public string? AmountType { get; set; }

    public string? Description { get; set; }        // Purpose of payment

    public string? Purpose { get; set; }            // short display purpose

    public string? CustomerEmail { get; set; }

    public string? CustomerPhone { get; set; }

    public string? CustomerName { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public DateTime? DueDate { get; set; }

    public string? Status { get; set; }             // created, active, deactivated, expired, paid, cancelled

    public string? PaymentType { get; set; }        // Standard, PartialPayment

    public bool? IsPartialPayment { get; set; }

    public int? MaxPaymentsAllowed { get; set; }    // max partial payment count

    public int? ValidationPeriod { get; set; }      // numeric validity period

    public string? TimeUnit { get; set; }           // D, H, M

    public bool? SendSms { get; set; }

    public string? ShortUrl { get; set; }

    public string? ReferenceId { get; set; }        // merchant / invoice reference

    public string? InvoiceId { get; set; }          // external invoice id

    public int? MaxUses { get; set; }

    public int UsedCount { get; set; }

    public int TotalViews { get; set; }

    public decimal TotalAmountPaid { get; set; }

    public string? CustomerDataCapture { get; set; } // JSON array of data-capture fields

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual Merchant MidNavigation { get; set; } = null!;

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
