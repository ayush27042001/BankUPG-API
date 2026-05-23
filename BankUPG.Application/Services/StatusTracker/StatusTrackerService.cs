using BankUPG.Application.Interfaces.StatusTracker;
using BankUPG.Infrastructure.Data;
using BankUPG.Infrastructure.Entities;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;

namespace BankUPG.Application.Services.StatusTracker
{
    public class StatusTrackerService : IStatusTrackerService
    {
        private readonly AppDBContext _context;

        public StatusTrackerService(AppDBContext context)
        {
            _context = context;
        }

        public async Task<StatusTrackerResponse?> GetOnboardingStatusAsync(int userId)
        {
            var merchant = await _context.Merchants
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (merchant == null) return null;

            var mid = merchant.Mid;
            var steps = await BuildStatusStepsAsync(mid, merchant);

            var approvedCount = steps.Count(s => s.Status == "approved");
            var inProgressCount = steps.Count(s => s.Status == "in-progress");
            var overallProgress = steps.Count > 0
                ? (int)Math.Round((approvedCount + inProgressCount * 0.5) / steps.Count * 100)
                : 0;

            return new StatusTrackerResponse
            {
                ApplicationId = $"APP-{(merchant.CreatedDate ?? DateTime.UtcNow).Year}-{mid:D5}",
                StatusSteps = steps,
                OverallProgress = overallProgress,
                LastUpdated = DateTime.UtcNow.ToString("o"),
                IsOnboardingCompleted = merchant.IsOnboardingCompleted ?? false,
                IsOnboardingRejected = merchant.IsOnboardingRejected ?? false
            };
        }

        private async Task<List<StatusTrackerStepDto>> BuildStatusStepsAsync(int mid, Merchant merchant)
        {
            var hasWebsiteApp = await _context.WebsiteAppDetails.AnyAsync(w => w.Mid == mid);
            var hasBankAccount = await _context.BankAccountDetails.AnyAsync(b => b.Mid == mid);
            var hasSigningAuthority = await _context.SigningAuthorityDetails.AnyAsync(s => s.Mid == mid);

            var requiredDocTypeIds = await _context.DocumentTypes
                .Where(dt => dt.IsRequired == true && dt.IsActive == true)
                .Select(dt => dt.DocumentTypeId)
                .ToListAsync();

            var uploadedDocTypeIds = await _context.DocumentUploads
                .Where(du => du.Mid == mid)
                .Select(du => du.DocumentTypeId)
                .Distinct()
                .ToListAsync();

            bool allRequiredDocsUploaded = requiredDocTypeIds.Count > 0
                && requiredDocTypeIds.All(id => uploadedDocTypeIds.Contains(id));
            bool anyDocsUploaded = uploadedDocTypeIds.Count > 0;

            var videoKyc = await _context.VideoKycdetails
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Mid == mid);

            var hasServiceAgreement = await _context.ServiceAgreements.AnyAsync(sa => sa.Mid == mid);
            bool isOnboardingCompleted = merchant.IsOnboardingCompleted ?? false;
            bool isOnboardingRejected = merchant.IsOnboardingRejected ?? false;

            // Step 1: Business Information
            string businessInfoStatus;
            if (hasWebsiteApp && hasBankAccount) businessInfoStatus = "approved";
            else if (hasWebsiteApp || hasBankAccount) businessInfoStatus = "in-progress";
            else businessInfoStatus = "pending";

            // Step 2: KYC Checks
            string kycStatus = hasSigningAuthority ? "approved" : "pending";

            // Step 3: Document Verification
            string docStatus;
            if (allRequiredDocsUploaded) docStatus = "approved";
            else if (anyDocsUploaded) docStatus = "in-progress";
            else docStatus = "pending";

            // Step 4: Video KYC
            string videoKycStatus;
            if (videoKyc != null && videoKyc.VideoKycstatus == "COMPLETED") videoKycStatus = "approved";
            else if (videoKyc != null) videoKycStatus = "in-progress";
            else videoKycStatus = "pending";

            // Step 5: Service Agreement
            string agreementStatus = hasServiceAgreement ? "approved" : "pending";

            // Step 6: Final Approval
            string finalStatus;
            if (isOnboardingRejected) finalStatus = "rejected";
            else if (isOnboardingCompleted) finalStatus = "approved";
            else finalStatus = "pending";

            return new List<StatusTrackerStepDto>
            {
                new()
                {
                    Id = "business-info",
                    Title = "Business Information",
                    Description = "Platform details and bank account verification",
                    Status = businessInfoStatus,
                    Icon = "🏢",
                    Remarks = businessInfoStatus == "approved" ? "Verified successfully" : null
                },
                new()
                {
                    Id = "kyc-checks",
                    Title = "KYC Checks",
                    Description = "Signing authority verification and PEP status",
                    Status = kycStatus,
                    Icon = "🔍",
                    Remarks = kycStatus == "approved" ? "All KYC documents verified" : null
                },
                new()
                {
                    Id = "documents",
                    Title = "Document Verification",
                    Description = "Aadhaar, PAN, business proof, and shop photos",
                    Status = docStatus,
                    Icon = "📄",
                    Remarks = docStatus == "in-progress" ? "Under review by compliance team" : null
                },
                new()
                {
                    Id = "video-kyc",
                    Title = "Video KYC",
                    Description = "Live verification with KYC agent",
                    Status = videoKycStatus,
                    Icon = "📹",
                    Remarks = videoKycStatus == "pending" ? "Scheduled after document approval" : null
                },
                new()
                {
                    Id = "agreement",
                    Title = "Service Agreement",
                    Description = "Digital signature and agreement acceptance",
                    Status = agreementStatus,
                    Icon = "✍️",
                    Remarks = agreementStatus == "pending" ? "Pending previous step completion" : null
                },
                new()
                {
                    Id = "final-approval",
                    Title = "Final Approval",
                    Description = "Account activation and payment gateway setup",
                    Status = finalStatus,
                    Icon = "🚀",
                    Remarks = finalStatus == "pending" ? "Final review by admin team" : null
                }
            };
        }
    }
}
