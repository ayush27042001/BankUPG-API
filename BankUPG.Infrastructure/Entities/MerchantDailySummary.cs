using System;

namespace BankUPG.Infrastructure.Entities;

public partial class MerchantDailySummary
{
    public long MerchantDailySummaryId { get; set; }

    public int Mid { get; set; }

    public DateTime SummaryDate { get; set; }

    public int TotalTransactions { get; set; }

    public decimal TotalTransactionAmount { get; set; }

    public int SuccessfulTransactions { get; set; }

    public decimal TotalMdrCharges { get; set; }

    public decimal TotalGst { get; set; }

    public decimal TotalDeductions { get; set; }

    public decimal TotalSettledAmount { get; set; }

    public decimal PendingSettlementAmount { get; set; }

    public int TotalRefunds { get; set; }

    public decimal TotalRefundAmount { get; set; }

    public int TotalChargebacks { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual Merchant MidNavigation { get; set; } = null!;
}
