using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.PepstatusMaster
{
    public interface IPepstatusMasterService
    {
        Task<List<PepstatusDto>> GetAllPepstatusesAsync();
    }
}
