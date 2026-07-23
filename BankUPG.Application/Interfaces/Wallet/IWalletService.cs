using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.Wallet
{
    public interface IWalletService
    {
        Task<WalletBalanceResponse?> GetWalletAsync(int userId);
        Task<PagedResponse<WalletLedgerItemResponse>> GetLedgerAsync(int userId, GetWalletLedgerRequest request);
    }
}
