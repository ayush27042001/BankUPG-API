using System;

namespace BankUPG.SharedKernal.Requests
{
    public class GetWalletLedgerRequest
    {
        public string? EntryType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
