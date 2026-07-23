using System.ComponentModel.DataAnnotations;

namespace BankUPG.SharedKernal.Requests
{
    public class CreateEmiPlanRequest
    {
        [Required]
        [MaxLength(200)]
        public string BankName { get; set; } = null!;

        public string? CardType { get; set; }

        [Required]
        [Range(1, 360)]
        public int Tenure { get; set; }

        [Range(0, 100)]
        public decimal InterestRate { get; set; }

        public decimal? MinAmount { get; set; }
    }

    public class UpdateEmiPlanRequest
    {
        public decimal? InterestRate { get; set; }
        public decimal? MinAmount { get; set; }
        public bool? IsActive { get; set; }
    }
}
