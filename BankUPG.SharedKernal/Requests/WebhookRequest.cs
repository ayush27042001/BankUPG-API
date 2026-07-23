using System;
using System.ComponentModel.DataAnnotations;

namespace BankUPG.SharedKernal.Requests
{
    public class CreateWebhookRequest
    {
        [Required]
        public int Mid { get; set; }

        [MaxLength(50)]
        public string? Type { get; set; }

        [MaxLength(100)]
        public string? Event { get; set; }

        [MaxLength(1000)]
        public string? WebhookUrl { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }

        [MaxLength(20)]
        public string? Status { get; set; } = "Active";
    }

    public class UpdateWebhookRequest : CreateWebhookRequest
    {
        [Required]
        public int WebhookId { get; set; }
    }
}
