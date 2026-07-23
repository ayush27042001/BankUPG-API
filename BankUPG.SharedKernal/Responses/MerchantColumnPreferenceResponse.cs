using System;

namespace BankUPG.SharedKernal.Responses
{
    public class MerchantColumnPreferenceResponse
    {
        public int MerchantColumnPreferenceId { get; set; }
        public int Mid { get; set; }
        public string? GridName { get; set; }
        public string? SelectedColumns { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
