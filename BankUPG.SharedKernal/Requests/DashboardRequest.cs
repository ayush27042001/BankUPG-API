using System;

namespace BankUPG.SharedKernal.Requests
{
    public class GetDashboardSummaryRequest
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
