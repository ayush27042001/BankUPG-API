namespace BankUPG.SharedKernal.Responses
{
    public class BusinessEntityTypeMasterResponse
    {
        public int BusinessEntityTypeId { get; set; }

        public string EntityName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}