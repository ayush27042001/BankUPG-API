namespace BankUPG.SharedKernal.Responses
{
    public class RegistrationInitiatedResponse
    {
        public string RegistrationToken { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public int OtpExpirySeconds { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class RegistrationCompletedResponse
    {
        public int UserId { get; set; }
        public int Mid { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? FormStep { get; set; }
        public int? Step { get; set; }
        public OnboardingStatusDto OnboardingStatus { get; set; } = new();
    }

    public class OnboardingStatusDto
    {
        public int StepNumber { get; set; }
        public string StepName { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public List<OnboardingStepDto> Steps { get; set; } = new();
        public ConnectPlatformStepsDto? ConnectPlatformSteps { get; set; }
    }

    public class ConnectPlatformStepsDto
    {
        public int CurrentStep { get; set; }
        public int TotalSteps { get; set; } = 5;
        public List<ConnectPlatformStepDto> Steps { get; set; } = new();
    }

    public class ConnectPlatformStepDto
    {
        public int StepNumber { get; set; }
        public string StepName { get; set; } = string.Empty;
        public string StepKey { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public bool IsActive { get; set; }
    }

    public class OnboardingStepDto
    {
        public int StepNumber { get; set; }
        public string StepName { get; set; } = string.Empty;
        public string StepKey { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public bool IsActive { get; set; }
    }

    public class OtpVerificationResponse
    {
        public bool IsVerified { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? RegistrationToken { get; set; }
        public int? RemainingAttempts { get; set; }
        
        // User details returned on successful verification (before user creation)
        public int? UserId { get; set; }
        public int? Mid { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? MobileNumber { get; set; }
        public string? CompanyWebsite { get; set; }
        public string? Token { get; set; }
        public DateTime? TokenExpiration { get; set; }
        
        // Form flow tracking
        public string? FormStep { get; set; }
        public int Step { get; set; }
    }

    public class ResendOtpResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int RemainingSeconds { get; set; }
        public int MaxResendAttempts { get; set; }
        public int RemainingResendAttempts { get; set; }
    }

    public class PanDetailsResponse
    {
        public string PanCardNumber { get; set; } = string.Empty;
        public string NameOnPanCard { get; set; } = string.Empty;
        public DateTime DateOfBirthOrIncorporation { get; set; }
        public string? VerificationStatus { get; set; }
        public int? MerchantId { get; set; }
    }

    public class BusinessEntityTypeDto
    {
        public int BusinessEntityTypeId { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class BusinessEntityResponse
    {
        public int Mid { get; set; }
        public int BusinessEntityTypeId { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class BusinessEntitySavedResponse
    {
        public int Mid { get; set; }
        public int BusinessEntityTypeId { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? FormStep { get; set; }
        public int? Step { get; set; }
        public OnboardingStatusDto OnboardingStatus { get; set; } = new();
    }

    public class PhoneCkycResponse
    {
        public int Mid { get; set; }
        public string MobileNumber { get; set; } = string.Empty;
        public string? CkycIdentifier { get; set; }
        public bool ConsentGiven { get; set; }
        public DateTime? ConsentDate { get; set; }
    }

    public class PhoneCkycSavedResponse
    {
        public int Mid { get; set; }
        public string? CkycIdentifier { get; set; }
        public bool ConsentGiven { get; set; }
        public DateTime? ConsentDate { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime TokenExpiration { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? FormStep { get; set; }
        public int? Step { get; set; }
        public OnboardingStatusDto OnboardingStatus { get; set; } = new();
    }

    public class BusinessSubCategoryDto
    {
        public int BusinessSubCategoryId { get; set; }
        public int BusinessCategoryId { get; set; }
        public string SubCategoryName { get; set; } = string.Empty;
        public string SubCategoryCode { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class BusinessCategoryDto
    {
        public int BusinessCategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<BusinessSubCategoryDto> SubCategories { get; set; } = new();
    }

    public class MerchantBusinessCategoryResponse
    {
        public int Mid { get; set; }
        public int? BusinessCategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int? BusinessSubCategoryId { get; set; }
        public string? SubCategoryName { get; set; }
    }

    public class BusinessCategorySavedResponse
    {
        public int Mid { get; set; }
        public int BusinessCategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int BusinessSubCategoryId { get; set; }
        public string SubCategoryName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? FormStep { get; set; }
        public int? Step { get; set; }
        public OnboardingStatusDto OnboardingStatus { get; set; } = new();
    }

    public class BusinessDetailsResponse
    {
        public int Mid { get; set; }
        public decimal? ExpectedSalesPerMonth { get; set; }
        public bool? HasGstin { get; set; }
        public string? Gstin { get; set; }
    }

    public class BusinessDetailsSavedResponse
    {
        public int Mid { get; set; }
        public decimal? ExpectedSalesPerMonth { get; set; }
        public bool? HasGstin { get; set; }
        public string? Gstin { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? FormStep { get; set; }
        public int? Step { get; set; }
        public OnboardingStatusDto OnboardingStatus { get; set; } = new();
    }

    public class GstVerifyResult
    {
        public bool IsValid { get; set; }
        public string? LegalName { get; set; }
        public string? TradeName { get; set; }
    }

    public class BankAccountDetailResponse
    {
        public int BankAccountDetailId { get; set; }
        public int Mid { get; set; }
        public string BankHolderName { get; set; } = string.Empty;
        public string BankAccountNumber { get; set; } = string.Empty;
        public string Ifsccode { get; set; } = string.Empty;
        public string? BankName { get; set; }
        public string? AccountType { get; set; }
        public bool? IsVerified { get; set; }
        public DateTime? VerifiedDate { get; set; }
    }

    public class BankAccountDetailSavedResponse
    {
        public int BankAccountDetailId { get; set; }
        public int Mid { get; set; }
        public string BankHolderName { get; set; } = string.Empty;
        public string BankAccountNumber { get; set; } = string.Empty;
        public string Ifsccode { get; set; } = string.Empty;
        public string? BankName { get; set; }
        public string? AccountType { get; set; }
        public string Message { get; set; } = string.Empty;
        public OnboardingStatusDto OnboardingStatus { get; set; } = new();
    }

    public class BankAccountVerifyResult
    {
        public bool IsValid { get; set; }
        public bool IsNameMatched { get; set; }
        public string? AccountStatus { get; set; }
        public string? NameAtBank { get; set; }
        public string? BankName { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class ConnectPlatformResponse
    {
        public int Mid { get; set; }
        public string PaymentCollectionPreference { get; set; } = string.Empty;
        public string? WebsiteAppUrl { get; set; }
        public string? AndroidAppUrl { get; set; }
        public string? IosAppUrl { get; set; }
    }

    public class ConnectPlatformSavedResponse
    {
        public int Mid { get; set; }
        public string PaymentCollectionPreference { get; set; } = string.Empty;
        public string? WebsiteAppUrl { get; set; }
        public string? AndroidAppUrl { get; set; }
        public string? IosAppUrl { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? FormStep { get; set; }
        public int? Step { get; set; }
        public OnboardingStatusDto OnboardingStatus { get; set; } = new();
    }

    public class SigningAuthorityDetailResponse
    {
        public int SigningAuthorityDetailId { get; set; }
        public int Mid { get; set; }
        public string SigningAuthorityName { get; set; } = string.Empty;
        public string SigningAuthorityEmail { get; set; } = string.Empty;
        public string SigningAuthorityPan { get; set; } = string.Empty;
        public int PepstatusId { get; set; }
        public string? PepstatusName { get; set; }
    }

    public class SigningAuthorityDetailSavedResponse
    {
        public int SigningAuthorityDetailId { get; set; }
        public int Mid { get; set; }
        public string SigningAuthorityName { get; set; } = string.Empty;
        public string SigningAuthorityEmail { get; set; } = string.Empty;
        public string SigningAuthorityPan { get; set; } = string.Empty;
        public int PepstatusId { get; set; }
        public string? PepstatusName { get; set; }
        public string Message { get; set; } = string.Empty;
        public OnboardingStatusDto OnboardingStatus { get; set; } = new();
    }

    public class PepstatusDto
    {
        public int PepstatusId { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class BusinessAddressResponse
    {
        public int BusinessAddressDetailId { get; set; }
        public int Mid { get; set; }
        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string? Country { get; set; }
        public bool? HasDifferentOperatingAddress { get; set; }
        public string? OperatingAddressLine1 { get; set; }
        public string? OperatingAddressLine2 { get; set; }
        public string? OperatingCity { get; set; }
        public string? OperatingState { get; set; }
        public string? OperatingPostalCode { get; set; }
        public string? OperatingCountry { get; set; }
    }

    public class BusinessAddressSavedResponse
    {
        public int BusinessAddressDetailId { get; set; }
        public int Mid { get; set; }
        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string? Country { get; set; }
        public bool? HasDifferentOperatingAddress { get; set; }
        public string? OperatingAddressLine1 { get; set; }
        public string? OperatingAddressLine2 { get; set; }
        public string? OperatingCity { get; set; }
        public string? OperatingState { get; set; }
        public string? OperatingPostalCode { get; set; }
        public string? OperatingCountry { get; set; }
        public string Message { get; set; } = string.Empty;
        public OnboardingStatusDto OnboardingStatus { get; set; } = new();
    }
}
