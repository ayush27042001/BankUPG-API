using System;

namespace BankUPG.SharedKernal.Responses
{
    public class UserResponse
    {
        public int UserId { get; set; }

        public string Email { get; set; } = string.Empty;

        public string? MobileNumber { get; set; }

        public bool? IsEmailVerified { get; set; }

        public bool? IsMobileVerified { get; set; }

        public bool? IsActive { get; set; }

        public bool? IsLocked { get; set; }

        public int? FailedLoginAttempts { get; set; }

        public DateTime? LastLoginDate { get; set; }

        public DateTime? PasswordLastChangedDate { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}