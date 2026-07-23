using System;

namespace BankUPG.SharedKernal.Responses
{
    public class WebhookResponse
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
    }
}
