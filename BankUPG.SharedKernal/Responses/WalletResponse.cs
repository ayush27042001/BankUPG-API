using System;

namespace BankUPG.SharedKernal.Responses
{
    public class WalletBalanceResponse
    {
        public int Mid { get; set; }
        public decimal TotalBalance { get; set; }
        public decimal OnHoldBalance { get; set; }
        public decimal RefundWalletBalance { get; set; }
        public decimal AvailableBalance => TotalBalance - OnHoldBalance;
        public decimal TotalCredited { get; set; }
        public decimal TotalDebited { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    public class WalletLedgerItemResponse
    {
        public long WalletLedgerId { get; set; }
        public string? ReferenceType { get; set; }
        public long? ReferenceId { get; set; }
        public string? EntryType { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public string? Description { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
