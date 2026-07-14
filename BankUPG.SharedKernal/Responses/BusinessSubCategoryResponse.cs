using System;

namespace BankUPG.SharedKernal.Responses
{
    public class BusinessSubCategoryResponse
    {
        public int BusinessSubCategoryId { get; set; }

        public int BusinessCategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public string SubCategoryName { get; set; } = string.Empty;

        public string SubCategoryCode { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}