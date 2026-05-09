using System.ComponentModel.DataAnnotations;

namespace BankUPG.SharedKernal.Requests
{
    public class SaveBankAccountDetailRequest
    {
        [Required(ErrorMessage = "Bank holder name is required")]
        [MaxLength(200, ErrorMessage = "Bank holder name cannot exceed 200 characters")]
        public string BankHolderName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Bank account number is required")]
        [MaxLength(20, ErrorMessage = "Bank account number cannot exceed 20 characters")]
        [RegularExpression(@"^\d{9,18}$", ErrorMessage = "Invalid bank account number format")]
        public string BankAccountNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "IFSC code is required")]
        [MaxLength(11, ErrorMessage = "IFSC code cannot exceed 11 characters")]
        [RegularExpression(@"^[A-Z]{4}0[A-Z0-9]{6}$", ErrorMessage = "Invalid IFSC code format")]
        public string Ifsccode { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "Bank name cannot exceed 100 characters")]
        public string? BankName { get; set; }

        [MaxLength(50, ErrorMessage = "Account type cannot exceed 50 characters")]
        [RegularExpression(@"^(SAVINGS|CURRENT|OVERDRAFT)$",
            ErrorMessage = "Invalid account type. Allowed values: SAVINGS, CURRENT, OVERDRAFT")]
        public string? AccountType { get; set; }
    }

    public class VerifyBankAccountRequest
    {
        [Required(ErrorMessage = "Bank account number is required")]
        public string AccountNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "IFSC code is required")]
        public string IFSCCode { get; set; } = string.Empty;

        public string? AccountHolderName { get; set; }

        public string? PhoneNumber { get; set; }
    }
}
