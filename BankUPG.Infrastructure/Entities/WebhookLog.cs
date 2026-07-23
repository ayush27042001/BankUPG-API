using System;

namespace BankUPG.Infrastructure.Entities;

public partial class WebhookLog
{
    public long WebhookLogId { get; set; }

    public int WebhookId { get; set; }

    public int Mid { get; set; }

    public string? TransactionReference { get; set; }

    public string? EventType { get; set; }

    public string? ErrorMessage { get; set; }

    public string? Status { get; set; }

    public int? HttpCode { get; set; }

    public DateTime? LogDate { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual Webhook Webhook { get; set; } = null!;

    public virtual Merchant MidNavigation { get; set; } = null!;
}
