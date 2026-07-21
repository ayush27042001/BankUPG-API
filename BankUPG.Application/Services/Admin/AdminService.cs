using BankUPG.Application.Interfaces.Admin;
using BankUPG.Infrastructure.Data;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;

namespace BankUPG.Application.Services.Admin
{
    public class AdminService : IAdminService
    {
        private readonly AppDBContext _context;

        public AdminService(AppDBContext context)
        {
            _context = context;
        }

        public async Task<UserCompleteDataResponse?> GetUserCompleteDataAsync(UserDetailRequest request)
        {
            var user = await _context.Users
                .AsNoTracking()
                .Include(u => u.Merchants)
                    .ThenInclude(m => m.BusinessCategory)
                .Include(u => u.Merchants)
                    .ThenInclude(m => m.BusinessSubCategory)
                .Include(u => u.Merchants)
                    .ThenInclude(m => m.BusinessEntityType)
                .Include(u => u.Merchants)
                    .ThenInclude(m => m.OnboardingStatus)
                .Include(u => u.Merchants)
                    .ThenInclude(m => m.BusinessPandetail)
                .Include(u => u.Merchants)
                    .ThenInclude(m => m.BusinessAddressDetail)
                .Include(u => u.Merchants)
                    .ThenInclude(m => m.BankAccountDetail)
                .Include(u => u.Merchants)
                    .ThenInclude(m => m.SigningAuthorityDetail)
                    .ThenInclude(s => s!.Pepstatus)
                .Include(u => u.Merchants)
                    .ThenInclude(m => m.WebsiteAppDetail)
                .Include(u => u.Merchants)
                    .ThenInclude(m => m.ServiceAgreement)
                .FirstOrDefaultAsync(u => u.UserId == request.UserId);

            if (user == null)
                return null;

            var merchant = user.Merchants.FirstOrDefault();

            var documents = new List<UserDocumentResponse>();
            if (merchant != null)
            {
                documents = await _context.DocumentUploads
                    .AsNoTracking()
                    .Where(d => d.Mid == merchant.Mid)
                    .Include(d => d.DocumentType)
                    .Include(d => d.BusinessProofType)
                    .Select(d => new UserDocumentResponse
                    {
                        MerchantDetail = (user.MobileNumber ?? "") + "/" + merchant.Mid + "/" + (user.Email ?? ""),
                        DocumentName = d.DocumentType.DocumentName,
                        IsRequired = d.DocumentType.IsRequired,
                        MaxFileSizeMB = d.DocumentType.MaxFileSizeMb,
                        AllowedExtensions = d.DocumentType.AllowedExtensions,
                        DocumentFileName = d.DocumentFileName,
                        DocumentSizeBytes = d.DocumentSizeBytes,
                        IsVerified = d.IsVerified,
                        RejectionReason = d.RejectionReason,
                        VerifiedBy = d.VerifiedBy,
                        VerifiedDate = d.VerifiedDate,
                        DocumentFilePath = d.DocumentFilePath,
                        DocumentMimeType = d.DocumentMimeType,
                        DocumentUploadID = d.DocumentUploadId,
                        ProofName = d.BusinessProofType != null ? d.BusinessProofType.ProofName : null
                    })
                    .ToListAsync();
            }

            return new UserCompleteDataResponse
            {
                UserId = user.UserId,
                LastLoginDate = user.LastLoginDate,
                Email = user.Email,
                IsLocked = user.IsLocked == true ? "Locked" : "Not Locked",
                Active = user.IsActive == true ? "Active" : "In Active",
                EmailVerification = user.IsEmailVerified == true ? "Email Verified" : "Not Verified",
                IsMobileVerified = user.IsMobileVerified == true ? "Mobile Verified" : "Not Verified",
                MobileNumber = user.MobileNumber,
                BusinessName = merchant?.BusinessName,
                NameOnPANCard = merchant?.BusinessPandetail?.NameOnPancard,
                DateOfBirthOrIncorporation = merchant?.BusinessPandetail != null && merchant.BusinessPandetail.DateOfBirthOrIncorporation != default
                    ? merchant.BusinessPandetail.DateOfBirthOrIncorporation.ToString("yyyy-MM-dd")
                    : null,
                PANCardNumber = merchant?.BusinessPandetail?.PancardNumber,
                PANVerificationStatus = merchant?.BusinessPandetail?.PanverificationStatus,
                PANVerifiedDate = merchant?.BusinessPandetail?.PanverifiedDate,
                BusinessEntityName = merchant?.BusinessEntityType?.EntityName,
                CKYCConsentGiven = merchant?.CkycconsentGiven,
                CKYCIdentifier = merchant?.Ckycidentifier,
                BusinessCategoryName = merchant?.BusinessCategory?.CategoryName,
                BusinessSubCategoryName = merchant?.BusinessSubCategory?.SubCategoryName,
                ExpectedSalesPerMonth = merchant?.ExpectedSalesPerMonth,
                GSTIN = merchant?.HasGstin == true ? merchant.Gstin : "GST Not Required",
                IsOnboardingCompleted = merchant?.IsOnboardingCompleted == true ? "Onboarding Completed" : "Not Completed",
                IsOnboardingRejected = merchant?.IsOnboardingRejected == true ? "Onboarding Rejected" : "Not Rejected",
                OnboardingStatus = merchant?.OnboardingStatus != null
                    ? $"{merchant.OnboardingStatus.StatusName}-{merchant.OnboardingStatus.StatusDescription}"
                    : null,
                AccountType = merchant?.BankAccountDetail?.AccountType,
                BankAccountNumber = merchant?.BankAccountDetail?.BankAccountNumber,
                BankHolderName = merchant?.BankAccountDetail?.BankHolderName,
                BankName = merchant?.BankAccountDetail?.BankName,
                IFSCCode = merchant?.BankAccountDetail?.Ifsccode,
                IsVerified = merchant?.BankAccountDetail?.IsVerified,
                VerifiedDate = merchant?.BankAccountDetail?.VerifiedDate,
                PEPStatus = merchant?.SigningAuthorityDetail?.Pepstatus?.StatusName,
                SigningAuthorityEmail = merchant?.SigningAuthorityDetail?.SigningAuthorityEmail,
                SigningAuthorityName = merchant?.SigningAuthorityDetail?.SigningAuthorityName,
                SigningAuthorityPAN = merchant?.SigningAuthorityDetail?.SigningAuthorityPan,
                AddressLine1 = merchant?.BusinessAddressDetail?.AddressLine1,
                AddressLine2 = merchant?.BusinessAddressDetail?.AddressLine2,
                City = merchant?.BusinessAddressDetail?.City,
                Country = merchant?.BusinessAddressDetail?.Country,
                PostalCode = merchant?.BusinessAddressDetail?.PostalCode,
                CreatedDate = merchant?.BusinessAddressDetail?.CreatedDate,
                HasDifferentOperatingAddress = merchant?.BusinessAddressDetail?.HasDifferentOperatingAddress,
                OperatingAddressLine1 = merchant?.BusinessAddressDetail?.OperatingAddressLine1,
                OperatingAddressLine2 = merchant?.BusinessAddressDetail?.OperatingAddressLine2,
                OperatingCity = merchant?.BusinessAddressDetail?.OperatingCity,
                OperatingCountry = merchant?.BusinessAddressDetail?.OperatingCountry,
                OperatingPostalCode = merchant?.BusinessAddressDetail?.OperatingPostalCode,
                OperatingState = merchant?.BusinessAddressDetail?.OperatingState,
                WebsiteAppURL = merchant?.WebsiteAppDetail?.WebsiteAppUrl,
                PaymentCollectionPreference = merchant?.WebsiteAppDetail?.PaymentCollectionPreference,
                AndroidAppURL = merchant?.WebsiteAppDetail?.AndroidAppUrl,
                iOSAppURL = merchant?.WebsiteAppDetail?.IOsappUrl,
                AgreementDate = merchant?.ServiceAgreement?.AgreementDate,
                IsAccepted = merchant?.ServiceAgreement?.IsAccepted,
                SignatureData = merchant?.ServiceAgreement?.SignatureData,
                SubmittedDate = merchant?.ServiceAgreement?.SubmittedDate,
                Documents = documents
            };
        }
    }
}
