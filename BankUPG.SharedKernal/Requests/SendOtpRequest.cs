using System.ComponentModel.DataAnnotations;

namespace BankUPG.SharedKernal.Requests
{
    public class SendOtpRequest
    {
        [Required(ErrorMessage = "Mobile number is required")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Mobile number must be 10 digits")]
        public string MobileNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Purpose is required")]
        public string Purpose { get; set; } = string.Empty; // LOGIN, REGISTRATION, PASSWORD_RESET
    }
}
