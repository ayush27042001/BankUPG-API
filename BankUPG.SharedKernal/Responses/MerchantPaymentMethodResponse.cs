using System;

namespace BankUPG.SharedKernal.Responses
{
    public class MerchantPaymentMethodResponse
    {
        public int MerchantPaymentMethodId { get; set; }
        public int Mid { get; set; }
        public string? PaymentMethodType { get; set; }
        public string? SubMethodName { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
