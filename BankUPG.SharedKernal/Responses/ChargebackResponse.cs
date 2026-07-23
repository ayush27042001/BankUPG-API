using System;

namespace BankUPG.SharedKernal.Responses
{
    public class ChargebackResponse
    {
        // User requested columns
        public long ChargebackId { get; set; }
        public string? View => HasDocuments ? DocumentPath : null;   // action/view link if docs uploaded
        public long? TransactionId { get; set; }
        public DateTime? ChargebackDate { get; set; }
        public DateTime? ReplyBefore { get; set; }
        public string? Status { get; set; }
        public string? CaseNumber { get; set; }
        public string? ChargebackReason { get; set; }
        public string? Documents => DocumentPath;                    // document path / URL
        public bool HasDocuments => !string.IsNullOrEmpty(DocumentPath);

        // Extra available columns
        public int Mid { get; set; }
        public string? PayuId { get; set; }
        public string? BankCaseNumber { get; set; }
        public string? ChargebackType { get; set; }
        public string? CloseReason { get; set; }
        public string? DocumentPath { get; set; }
        public bool IsOverdue => ReplyBefore.HasValue && DateTime.UtcNow > ReplyBefore.Value && Status != "Closed";
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
