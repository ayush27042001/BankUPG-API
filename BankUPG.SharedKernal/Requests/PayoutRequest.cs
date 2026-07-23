using System;
using System.ComponentModel.DataAnnotations;

namespace BankUPG.SharedKernal.Requests
{
    public class CreatePayoutBeneficiaryRequest
    {
        [Required]
        [MaxLength(200)]
        public string BeneficiaryName { get; set; } = null!;

        public string? AccountNumber { get; set; }
        public string? Ifsccode { get; set; }
        public string? BankName { get; set; }
        public string? AccountType { get; set; }
        public string? UpiId { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }

    public class CreatePayoutRequest
    {
        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        public long? BeneficiaryId { get; set; }

        [Required]
        public string Mode { get; set; } = null!;

        public string? Currency { get; set; } = "INR";
        public string? Narration { get; set; }
        public string? ReferenceId { get; set; }
        public DateTime? ScheduledDate { get; set; }
    }

    public class ListPayoutsRequest
    {
        public string? Status { get; set; }
        public string? Mode { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
