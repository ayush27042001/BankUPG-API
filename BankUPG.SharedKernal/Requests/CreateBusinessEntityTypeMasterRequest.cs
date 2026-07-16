using System.ComponentModel.DataAnnotations;

namespace BankUPG.SharedKernal.Requests
{
    public class CreateBusinessEntityTypeMasterRequest
    {
        [Required]
        [StringLength(100)]
        public string EntityName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }
    }
}