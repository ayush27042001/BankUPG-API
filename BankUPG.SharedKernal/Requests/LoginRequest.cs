using System.ComponentModel.DataAnnotations;

namespace BankUPG.SharedKernal.Requests
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        [MaxLength(128, ErrorMessage = "Password cannot exceed 128 characters")]
        public string Password { get; set; } = string.Empty;
    }
}
