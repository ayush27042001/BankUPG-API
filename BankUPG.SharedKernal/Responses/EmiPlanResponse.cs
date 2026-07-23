using System;

namespace BankUPG.SharedKernal.Responses
{
    public class EmiPlanResponse
    {
        public int EmiPlanId { get; set; }
        public int Mid { get; set; }
        public string? BankName { get; set; }
        public string? CardType { get; set; }
        public int Tenure { get; set; }
        public decimal InterestRate { get; set; }
        public decimal? MinAmount { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
