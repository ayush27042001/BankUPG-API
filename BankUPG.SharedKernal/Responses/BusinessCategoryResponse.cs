namespace BankUPG.SharedKernal.Responses
{
    public class BusinessCategoryResponse
    {
        public int BusinessCategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public string CategoryCode { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}