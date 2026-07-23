using System;

namespace BankUPG.Infrastructure.Entities;

public partial class TransactionCharge
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

    public virtual Transaction Transaction { get; set; } = null!;

    public virtual Merchant MidNavigation { get; set; } = null!;

    public virtual PaymentMethodCharge? PaymentMethodCharge { get; set; }
}
