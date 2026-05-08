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
        Completed = 8
    }

    public enum ConnectPlatformStepEnum
    {
        ConnectMobileAppOrWebsite = 1,
        ShareBankAccountDetails = 2,
        SigningAuthorityDetails = 3,
        VerifyBusinessAddress = 4,
        CompleteVideoKYC = 5,
        ServiceAgreement = 6
    }
}
