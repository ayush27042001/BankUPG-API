using System;

namespace BankUPG.Infrastructure.Entities;

public partial class Settlement
{
    public long SettlementId { get; set; }

    public int Mid { get; set; }

    public string? UtrNumber { get; set; }

    public decimal? SalesAmount { get; set; }

    public decimal? Fees { get; set; }

    public decimal? SettledAmount { get; set; }

    public string? Status { get; set; }

    public string? SettlementCycle { get; set; }

    public int? SettlementT { get; set; }

    public DateTime? SettlementDate { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual Merchant MidNavigation { get; set; } = null!;
}
