namespace BankUPG.SharedKernal.Responses
{
    public class BusinessProofTypeMasterResponse
    {
        public int BusinessProofTypeId { get; set; }
        public string ProofName { get; set; } = string.Empty;
        public string ProofCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    public class BusinessProofTypeMasterListResponse
    {
        public List<BusinessProofTypeMasterResponse> BusinessProofTypes { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
