using System;
using System.Collections.Generic;

namespace BankUPG.Infrastructure.Entities;

public partial class MerchantWallet
{
    public int MerchantWalletId { get; set; }

    public int Mid { get; set; }

    public decimal TotalBalance { get; set; }

    public decimal OnHoldBalance { get; set; }

    public decimal RefundWalletBalance { get; set; }

    public decimal TotalCredited { get; set; }

    public decimal TotalDebited { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual Merchant MidNavigation { get; set; } = null!;

    public virtual ICollection<WalletLedger> WalletLedgers { get; set; } = new List<WalletLedger>();
}
