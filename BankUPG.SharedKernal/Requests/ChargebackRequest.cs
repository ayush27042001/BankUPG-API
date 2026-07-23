using System;

namespace BankUPG.SharedKernal.Requests
{
    public class ListChargebacksRequest
    {
        public long? TransactionId { get; set; }
        public string? BankCaseNumber { get; set; }
        public string? CaseNumber { get; set; }
        public string? Status { get; set; }            // New, PendingResponse, PendingReview, Closed
        public string? DateFilter { get; set; }        // today, yesterday, last7days, last30days, custom
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? ChargebackType { get; set; }    // CB, GoodFaith, PreArb
        public string? CloseReason { get; set; }       // Accepted, Rejected, Resolved

        public string? SortBy { get; set; }
        public string? SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class UpdateChargebackRequest
    {
        public string? Status { get; set; }
        public string? CloseReason { get; set; }
        public string? DocumentPath { get; set; }
    }
}
