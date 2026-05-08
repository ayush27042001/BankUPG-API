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

    public class SaveBusinessEntityRequest
    {
        [Required(ErrorMessage = "Business entity type is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid business entity type")]
        public int BusinessEntityTypeId { get; set; }
    }

    public class SavePhoneCkycRequest
    {
        [MaxLength(50, ErrorMessage = "CKYC identifier cannot exceed 50 characters")]
        public string? CkycIdentifier { get; set; }

        [Required(ErrorMessage = "Consent flag is required")]
        public bool ConsentGiven { get; set; }
    }

    public class SaveBusinessCategoryRequest
    {
        [Required(ErrorMessage = "Business category is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid business category")]
        public int BusinessCategoryId { get; set; }

        [Required(ErrorMessage = "Business sub-category is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid business sub-category")]
        public int BusinessSubCategoryId { get; set; }
    }

    public class SaveBusinessDetailsRequest
    {
        [Required(ErrorMessage = "Expected sales per month is required")]
        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Expected sales must be greater than 0")]
        public decimal ExpectedSalesPerMonth { get; set; }

        [Required(ErrorMessage = "HasGSTIN flag is required")]
        public bool HasGstin { get; set; }

        [MaxLength(15, ErrorMessage = "GSTIN cannot exceed 15 characters")]
        [RegularExpression(@"^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$",
            ErrorMessage = "Invalid GSTIN format")]
        public string? Gstin { get; set; }
    }

    public class ValidateGstRequest
    {
        [Required(ErrorMessage = "GSTIN is required")]
        [MaxLength(15, ErrorMessage = "GSTIN cannot exceed 15 characters")]
        [RegularExpression(@"^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$",
            ErrorMessage = "Invalid GSTIN format")]
        public string Gstin { get; set; } = string.Empty;

        [MaxLength(200, ErrorMessage = "Business name cannot exceed 200 characters")]
        public string? BusinessName { get; set; }
    }

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

    public class SaveConnectPlatformRequest : IValidatableObject
    {
        [Required(ErrorMessage = "Payment collection preference is required")]
        [RegularExpression(@"^(ON_MY_WEBSITE_APP|WITHOUT_WEBSITE_APP)$",
            ErrorMessage = "Invalid payment collection preference. Allowed values: ON_MY_WEBSITE_APP, WITHOUT_WEBSITE_APP")]
        public string PaymentCollectionPreference { get; set; } = string.Empty;

        [Url(ErrorMessage = "Invalid website/app URL format")]
        [MaxLength(500, ErrorMessage = "Website/App URL cannot exceed 500 characters")]
        public string? WebsiteAppUrl { get; set; }

        [Url(ErrorMessage = "Invalid Android app URL format")]
        [MaxLength(500, ErrorMessage = "Android App URL cannot exceed 500 characters")]
        public string? AndroidAppUrl { get; set; }

        [Url(ErrorMessage = "Invalid iOS app URL format")]
        [MaxLength(500, ErrorMessage = "iOS App URL cannot exceed 500 characters")]
        public string? IosAppUrl { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (PaymentCollectionPreference == "ON_MY_WEBSITE_APP" &&
                string.IsNullOrWhiteSpace(WebsiteAppUrl))
            {
                yield return new ValidationResult(
                    "Website/App URL is required when payment collection preference is 'On my website/app'.",
                    new[] { nameof(WebsiteAppUrl) });
            }
        }
    }

    public class SaveSigningAuthorityDetailRequest
    {
        [Required(ErrorMessage = "Signing authority name is required")]
        [MaxLength(200, ErrorMessage = "Signing authority name cannot exceed 200 characters")]
        public string SigningAuthorityName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Signing authority email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string SigningAuthorityEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Signing authority PAN is required")]
        [RegularExpression(@"^[A-Z]{5}[0-9]{4}[A-Z]{1}$", ErrorMessage = "Invalid PAN card format")]
        [MaxLength(10, ErrorMessage = "PAN cannot exceed 10 characters")]
        public string SigningAuthorityPan { get; set; } = string.Empty;

        [Required(ErrorMessage = "PEP status is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid PEP status")]
        public int PepstatusId { get; set; }
    }

    public class SaveBusinessAddressRequest
    {
        [Required(ErrorMessage = "Address line 1 is required")]
        [MaxLength(500, ErrorMessage = "Address line 1 cannot exceed 500 characters")]
        public string AddressLine1 { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Address line 2 cannot exceed 500 characters")]
        public string? AddressLine2 { get; set; }

        [Required(ErrorMessage = "City is required")]
        [MaxLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "State is required")]
        [MaxLength(100, ErrorMessage = "State cannot exceed 100 characters")]
        public string State { get; set; } = string.Empty;

        [Required(ErrorMessage = "Postal code is required")]
        [MaxLength(10, ErrorMessage = "Postal code cannot exceed 10 characters")]
        [RegularExpression(@"^[0-9]{6}$", ErrorMessage = "Invalid postal code format")]
        public string PostalCode { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
        public string? Country { get; set; }

        [Required(ErrorMessage = "Has different operating address flag is required")]
        public bool HasDifferentOperatingAddress { get; set; }

        [MaxLength(500, ErrorMessage = "Operating address line 1 cannot exceed 500 characters")]
        public string? OperatingAddressLine1 { get; set; }

        [MaxLength(500, ErrorMessage = "Operating address line 2 cannot exceed 500 characters")]
        public string? OperatingAddressLine2 { get; set; }

        [MaxLength(100, ErrorMessage = "Operating city cannot exceed 100 characters")]
        public string? OperatingCity { get; set; }

        [MaxLength(100, ErrorMessage = "Operating state cannot exceed 100 characters")]
        public string? OperatingState { get; set; }

        [MaxLength(10, ErrorMessage = "Operating postal code cannot exceed 10 characters")]
        [RegularExpression(@"^[0-9]{6}$", ErrorMessage = "Invalid operating postal code format")]
        public string? OperatingPostalCode { get; set; }

        [MaxLength(100, ErrorMessage = "Operating country cannot exceed 100 characters")]
        public string? OperatingCountry { get; set; }
    }
}
