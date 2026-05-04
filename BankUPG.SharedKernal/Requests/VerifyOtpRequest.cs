using System.ComponentModel.DataAnnotations;

namespace BankUPG.SharedKernal.Requests
{
    public class VerifyOtpRequest
    {
        [Required(ErrorMessage = "Mobile number is required")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Mobile number must be 10 digits")]
        public string MobileNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "OTP is required")]
        [RegularExpression(@"^[0-9]{6}$", ErrorMessage = "OTP must be 6 digits")]
        public string Otp { get; set; } = string.Empty;
    }
}
