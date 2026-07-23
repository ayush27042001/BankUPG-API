using System;

namespace BankUPG.SharedKernal.Responses
{
    public class PayoutBeneficiaryResponse
    {
        public long PayoutBeneficiaryId { get; set; }
        public int Mid { get; set; }
        public string? BeneficiaryName { get; set; }
        public string? AccountNumber { get; set; }
        public string? Ifsccode { get; set; }
        public string? BankName { get; set; }
        public string? AccountType { get; set; }
        public string? UpiId { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
    }

    public class PayoutResponse
    {
        public long PayoutId { get; set; }
        public int Mid { get; set; }
        public long? BeneficiaryId { get; set; }
        public string? BeneficiaryName { get; set; }
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
        public string? Mode { get; set; }
        public string? Status { get; set; }
        public string? ReferenceId { get; set; }
        public string? UtrNumber { get; set; }
        public string? Narration { get; set; }
        public string? FailureReason { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
