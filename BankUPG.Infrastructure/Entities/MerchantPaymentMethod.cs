using System;

namespace BankUPG.Infrastructure.Entities;

public partial class MerchantPaymentMethod
{
    public int MerchantPaymentMethodId { get; set; }

    public int Mid { get; set; }

    public string? PaymentMethodType { get; set; }

    public string? SubMethodName { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual Merchant MidNavigation { get; set; } = null!;
}
