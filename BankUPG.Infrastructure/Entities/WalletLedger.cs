using System;

namespace BankUPG.Infrastructure.Entities;

public partial class WalletLedger
{
    public long WalletLedgerId { get; set; }

    public int MerchantWalletId { get; set; }

    public int Mid { get; set; }

    public string? ReferenceType { get; set; }

    public long? ReferenceId { get; set; }

    public string? EntryType { get; set; }

    public decimal Amount { get; set; }

    public decimal BalanceAfter { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual MerchantWallet MerchantWallet { get; set; } = null!;

    public virtual Merchant MidNavigation { get; set; } = null!;
}
