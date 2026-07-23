using System;
using System.ComponentModel.DataAnnotations;

namespace BankUPG.SharedKernal.Requests
{
    public class CreateTransactionChargeRequest
    {
        [Required]
        public long TransactionId { get; set; }

        [Required]
        public int Mid { get; set; }

        public int? PaymentMethodChargeId { get; set; }

        [MaxLength(50)]
        public string? PaymentMethodType { get; set; }

        [MaxLength(50)]
        public string? NetworkName { get; set; }

        [MaxLength(50)]
        public string? ChargeType { get; set; }

        public decimal ChargeValue { get; set; }
        public decimal TransactionAmount { get; set; }
        public decimal ChargeAmount { get; set; }
        public decimal GstAmount { get; set; }
        public decimal TotalDeduction { get; set; }
        public decimal NetAmount { get; set; }
    }

    public class UpdateTransactionChargeRequest : CreateTransactionChargeRequest
    {
        [Required]
        public long TransactionChargeId { get; set; }
    }

    public class RecalculateTransactionChargeRequest
    {
        [Required]
        public long TransactionId { get; set; }
    }
}
