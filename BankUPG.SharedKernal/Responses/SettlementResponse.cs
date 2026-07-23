using System;

namespace BankUPG.SharedKernal.Responses
{
    public class SettlementResponse
    {
        // User-requested columns
        public DateTime? Date => SettlementDate ?? CreatedDate;
        public long SettlementId { get; set; }
        public string? UtrNumber { get; set; }
        public decimal? SalesAmount { get; set; }
        public decimal? Fees { get; set; }
        public decimal? SettledAmount { get; set; }
        public string? Status { get; set; }

        public string? SettlementCycle { get; set; }
        public int? SettlementT { get; set; }
        public string? SettlementCycleLabel => SettlementT.HasValue ? $"T+{SettlementT}" : null;
        public DateTime? SettlementDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int Mid { get; set; }
    }

    public class SettlementConfigResponse
    {
        public int MerchantSettlementConfigId { get; set; }
        public int Mid { get; set; }
        public int SettlementT { get; set; }
        public string? SettlementCycleLabel => $"T+{SettlementT}";
        public string? SettlementCycleType { get; set; }
        public bool IsActive { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    public class SettlementSummaryResponse
    {
        public decimal TotalSalesAmount { get; set; }
        public decimal TotalFees { get; set; }
        public decimal TotalSettledAmount { get; set; }
        public decimal PendingAmount { get; set; }
        public decimal LastSettledAmount { get; set; }     // most recent settled amount
        public decimal TotalSettlementPending { get; set; } // pending amount not yet settled
        public decimal UpcomingSettlementAmount { get; set; } // amount likely in next cycle
        public int TotalSettlements { get; set; }
        public int CurrentSettlementT { get; set; }
    }
}
