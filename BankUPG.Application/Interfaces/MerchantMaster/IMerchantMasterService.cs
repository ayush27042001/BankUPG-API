using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.MerchantMaster
{
    public interface IMerchantMasterService
    {
        Task<MerchantResponse> CreateMerchantAsync(CreateMerchantRequest request);

        Task<MerchantResponse?> GetMerchantByIdAsync(int merchantId);

        Task<PagedResponse<MerchantResponse>> GetMerchantListAsync(GetMerchantListRequest request);
    }
}