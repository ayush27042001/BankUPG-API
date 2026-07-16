namespace BankUPG.SharedKernal.Requests
{
    public class GetMerchantListRequest
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public string? Search { get; set; }

        public bool? IsActive { get; set; }

        public int? BusinessEntityTypeId { get; set; }

        public int? BusinessCategoryId { get; set; }
    }
}