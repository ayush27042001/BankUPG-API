using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.BusinessDetails
{
    public interface IBusinessDetailsService
    {
        Task<BusinessDetailsResponse?> GetBusinessDetailsAsync(int userId);
        Task<BusinessDetailsSavedResponse> SaveBusinessDetailsAsync(int userId, SaveBusinessDetailsRequest request);
        Task<GstVerifyResult> VerifyGstAsync(string gstin, string? businessName);
    }
}
