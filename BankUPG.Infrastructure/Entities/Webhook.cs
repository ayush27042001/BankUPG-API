using System;
using System.Collections.Generic;

namespace BankUPG.Infrastructure.Entities;

public partial class Webhook
{
    public int WebhookId { get; set; }

    public int Mid { get; set; }

    public string? Type { get; set; }

    public string? Event { get; set; }

    public string? WebhookUrl { get; set; }

    public string? Remarks { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual Merchant MidNavigation { get; set; } = null!;

    public virtual ICollection<WebhookLog> WebhookLogs { get; set; } = new List<WebhookLog>();
}
