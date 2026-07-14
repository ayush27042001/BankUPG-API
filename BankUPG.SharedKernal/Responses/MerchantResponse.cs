namespace BankUPG.SharedKernal.Responses
{
    public class MerchantResponse
    {
        public int Mid { get; set; }
        public int UserId { get; set; }
        public string? BusinessName { get; set; }
        public int? BusinessEntityTypeId { get; set; }
        public int? OnboardingStatusId { get; set; }
        public string? Ckycidentifier { get; set; }
        public decimal? ExpectedSalesPerMonth { get; set; }
        public string? Gstin { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}