using System;
using System.Collections.Generic;

namespace BankUPG.SharedKernal.Responses
{
    public class DashboardSummaryResponse
    {
        public decimal TotalTransactionAmount { get; set; }
        public int TotalTransactions { get; set; }
        public int SuccessfulTransactions { get; set; }
        public int FailedTransactions { get; set; }
        public decimal SuccessRate { get; set; }
        public decimal TotalMdrCharges { get; set; }
        public decimal TotalGst { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TotalSettledAmount { get; set; }
        public decimal PendingSettlementAmount { get; set; }
        public decimal TotalRefundAmount { get; set; }
        public int TotalRefunds { get; set; }
        public decimal WalletBalance { get; set; }
        public decimal OnHoldBalance { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    public class DailyMetricResponse
    {
        public DateTime SummaryDate { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalTransactionAmount { get; set; }
        public int SuccessfulTransactions { get; set; }
        public decimal TotalSettledAmount { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TotalRefundAmount { get; set; }
    }
}
