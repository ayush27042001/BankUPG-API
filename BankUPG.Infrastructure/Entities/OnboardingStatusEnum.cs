namespace BankUPG.Infrastructure.Entities
{
    public enum OnboardingStatusEnum
    {
        AccountCreation = 1,
        PanVerification = 2,
        BusinessEntity = 3,
        PhoneCKYC = 4,
        BusinessCategory = 5,
        ShareBusinessDetails = 6,
        ConnectPlatform = 7,
        UploadDocuments = 8,
        ServiceAgreement = 9,
        Completed = 10
    }

    public enum ConnectPlatformStepEnum
    {
        ConnectMobileAppOrWebsite = 1,
        ShareBankAccountDetails = 2,
        SigningAuthorityDetails = 3,
        VerifyBusinessAddress = 4,
        CompleteVideoKYC = 5
    }
}
