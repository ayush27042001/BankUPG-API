namespace BankUPG.SharedKernal.Responses
{
    public class UserCompleteDataResponse
    {
        public int UserId { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public string? Email { get; set; }
        public string? IsLocked { get; set; }
        public string? Active { get; set; }
        public string? EmailVerification { get; set; }
        public string? IsMobileVerified { get; set; }
        public string? MobileNumber { get; set; }
        public string? BusinessName { get; set; }
        public string? NameOnPANCard { get; set; }
        public string? DateOfBirthOrIncorporation { get; set; }
        public string? PANCardNumber { get; set; }
        public string? PANVerificationStatus { get; set; }
        public DateTime? PANVerifiedDate { get; set; }
        public string? BusinessEntityName { get; set; }
        public bool? CKYCConsentGiven { get; set; }
        public string? CKYCIdentifier { get; set; }
        public string? BusinessCategoryName { get; set; }
        public string? BusinessSubCategoryName { get; set; }
        public decimal? ExpectedSalesPerMonth { get; set; }
        public string? GSTIN { get; set; }
        public string? IsOnboardingCompleted { get; set; }
        public string? IsOnboardingRejected { get; set; }
        public string? OnboardingStatus { get; set; }
        public string? AccountType { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankHolderName { get; set; }
        public string? BankName { get; set; }
        public string? IFSCCode { get; set; }
        public bool? IsVerified { get; set; }
        public DateTime? VerifiedDate { get; set; }
        public string? PEPStatus { get; set; }
        public string? SigningAuthorityEmail { get; set; }
        public string? SigningAuthorityName { get; set; }
        public string? SigningAuthorityPAN { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool? HasDifferentOperatingAddress { get; set; }
        public string? OperatingAddressLine1 { get; set; }
        public string? OperatingAddressLine2 { get; set; }
        public string? OperatingCity { get; set; }
        public string? OperatingCountry { get; set; }
        public string? OperatingPostalCode { get; set; }
        public string? OperatingState { get; set; }
        public string? WebsiteAppURL { get; set; }
        public string? PaymentCollectionPreference { get; set; }
        public string? AndroidAppURL { get; set; }
        public string? iOSAppURL { get; set; }
        public DateTime? AgreementDate { get; set; }
        public bool? IsAccepted { get; set; }
        public string? SignatureData { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public List<UserDocumentResponse> Documents { get; set; } = new List<UserDocumentResponse>();
    }
}
