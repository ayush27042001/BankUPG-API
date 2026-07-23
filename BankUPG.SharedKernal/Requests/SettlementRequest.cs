using System;

namespace BankUPG.SharedKernal.Requests
{
    public class ListSettlementsRequest
    {
        // User-requested filters
        public string? UtrNumber { get; set; }
        public long? TransactionId { get; set; }
        public string? DateFilter { get; set; }    // today, yesterday, last7days, last30days, custom
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Status { get; set; }        // Settled, Processing, InProgress, OnHold
        public string? SettlementCycle { get; set; } // Priority, Regular
        public int? SettlementT { get; set; }

        public string? SortBy { get; set; }
        public string? SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class UpdateSettlementConfigRequest
    {
        public int SettlementT { get; set; }
        public string? SettlementCycleType { get; set; }
    }
}
