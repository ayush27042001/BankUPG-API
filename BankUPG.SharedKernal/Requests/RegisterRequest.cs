using System.ComponentModel.DataAnnotations;

namespace BankUPG.SharedKernal.Requests
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        [MaxLength(128, ErrorMessage = "Password cannot exceed 128 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Password must contain at least one uppercase, one lowercase, one number, and one special character")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mobile number is required")]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Invalid Indian mobile number format")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Mobile number must be 10 digits")]
        public string MobileNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Company website is required")]
        [Url(ErrorMessage = "Invalid URL format")]
        [MaxLength(255, ErrorMessage = "Website URL cannot exceed 255 characters")]
        public string CompanyWebsite { get; set; } = string.Empty;

        [Required(ErrorMessage = "Business name is required")]
        [MaxLength(200, ErrorMessage = "Business name cannot exceed 200 characters")]
        public string BusinessName { get; set; } = string.Empty;
    }

    public class InitiateRegistrationRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        [MaxLength(128)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Password must contain at least one uppercase, one lowercase, one number, and one special character")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mobile number is required")]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Invalid Indian mobile number format")]
        [StringLength(10, MinimumLength = 10)]
        public string MobileNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Company website is required")]
        [Url(ErrorMessage = "Invalid URL format")]
        [MaxLength(255)]
        public string CompanyWebsite { get; set; } = string.Empty;

        [Required(ErrorMessage = "Business name is required")]
        [MaxLength(200)]
        public string BusinessName { get; set; } = string.Empty;
    }

    public class VerifyRegistrationOtpRequest
    {
        [Required(ErrorMessage = "Mobile number is required")]
        [RegularExpression(@"^[6-9]\d{9}$")]
        public string MobileNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "OTP is required")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be 6 digits")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP must be 6 digits")]
        public string Otp { get; set; } = string.Empty;

        [Required(ErrorMessage = "Registration token is required")]
        public string RegistrationToken { get; set; } = string.Empty;
    }

    public class CompleteRegistrationRequest
    {
        [Required(ErrorMessage = "Registration token is required")]
        public string RegistrationToken { get; set; } = string.Empty;

        [Required(ErrorMessage = "PAN card number is required")]
        [RegularExpression(@"^[A-Z]{5}[0-9]{4}[A-Z]{1}$", ErrorMessage = "Invalid PAN card format")]
        public string PanCardNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Name on PAN card is required")]
        [MaxLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        public string NameOnPanCard { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of birth/incorporation is required")]
        public DateTime DateOfBirthOrIncorporation { get; set; }
    }

    public class CompletePanRegistrationRequest
    {
        [Required(ErrorMessage = "PAN card number is required")]
        [RegularExpression(@"^[A-Z]{5}[0-9]{4}[A-Z]{1}$", ErrorMessage = "Invalid PAN card format")]
        public string PanCardNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Name on PAN card is required")]
        [MaxLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        public string NameOnPanCard { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of birth/incorporation is required")]
        public DateTime DateOfBirthOrIncorporation { get; set; }
    }
}
