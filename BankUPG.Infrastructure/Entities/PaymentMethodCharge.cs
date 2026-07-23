using System;
using System.Collections.Generic;

namespace BankUPG.Infrastructure.Entities;

public partial class PaymentMethodCharge
{
    public int PaymentMethodChargeId { get; set; }

    public string? PaymentMethodType { get; set; }

    public string? NetworkName { get; set; }

    public string? ChargeType { get; set; }

    public decimal ChargeValue { get; set; }

    public decimal? MinCharge { get; set; }

    public decimal? MaxCharge { get; set; }

    public decimal GstPercentage { get; set; }

    public bool IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ICollection<TransactionCharge> TransactionCharges { get; set; } = new List<TransactionCharge>();
}
