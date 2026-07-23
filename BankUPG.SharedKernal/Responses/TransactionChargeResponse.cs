using System;

namespace BankUPG.SharedKernal.Responses
{
    public class TransactionChargeResponse
    {
        public long TransactionChargeId { get; set; }
        public long TransactionId { get; set; }
        public int Mid { get; set; }
        public int? PaymentMethodChargeId { get; set; }
        public string? PaymentMethodType { get; set; }
        public string? NetworkName { get; set; }
        public string? ChargeType { get; set; }
        public decimal ChargeValue { get; set; }
        public decimal TransactionAmount { get; set; }
        public decimal ChargeAmount { get; set; }
        public decimal GstAmount { get; set; }
        public decimal TotalDeduction { get; set; }
        public decimal NetAmount { get; set; }
        public DateTime? CreatedDate { get; set; }
    }

    public class MerchantApiKeyResponse
    {
        public int MerchantApiKeyId { get; set; }
        public int Mid { get; set; }
        public string? ApiKey { get; set; }
        public string? ApiSalt { get; set; }
        public string? ClientId { get; set; }
        public string? ClientSecretHash { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
