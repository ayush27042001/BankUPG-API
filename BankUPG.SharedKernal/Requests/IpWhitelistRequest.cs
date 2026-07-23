using System.ComponentModel.DataAnnotations;

namespace BankUPG.SharedKernal.Requests
{
    public class AddIpWhitelistRequest
    {
        [Required]
        [MaxLength(50)]
        [RegularExpression(
            @"^((25[0-5]|2[0-4]\d|[01]?\d\d?)\.){3}(25[0-5]|2[0-4]\d|[01]?\d\d?)(\/([0-9]|[1-2][0-9]|3[0-2]))?$|^([0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4}$",
            ErrorMessage = "Invalid IP address or CIDR range")]
        public string IpAddress { get; set; } = null!;

        [MaxLength(300)]
        public string? Description { get; set; }
    }

    public class ToggleIpWhitelistRequest
    {
        public bool Enabled { get; set; }
    }
}
