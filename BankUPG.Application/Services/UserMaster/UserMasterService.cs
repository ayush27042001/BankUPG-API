using BankUPG.Application.Interfaces.UserMaster;
using BankUPG.Infrastructure.Data;
using BankUPG.Infrastructure.Entities;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankUPG.Application.Services.UserMaster
{
    public class UserMasterService : IUserMasterService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<UserMasterService> _logger;

        public UserMasterService(
            AppDBContext context,
            ILogger<UserMasterService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
        {
            // Check Email already exists
            var emailExists = await _context.Users
                .AnyAsync(x => x.Email == request.Email);

            if (emailExists)
                throw new InvalidOperationException($"User with email '{request.Email}' already exists.");

            // Check Mobile Number already exists
            var mobileExists = await _context.Users
                .AnyAsync(x => x.MobileNumber == request.MobileNumber);

            if (mobileExists)
                throw new InvalidOperationException($"User with mobile number '{request.MobileNumber}' already exists.");

            var user = new User
            {
                Email = request.Email,
                PasswordHash = request.Password,   // TODO: Replace with hashed password
                Salt = string.Empty,               // TODO: Generate salt later
                MobileNumber = request.MobileNumber,

                IsEmailVerified = false,
                IsMobileVerified = false,
                IsActive = true,
                IsLocked = false,
                FailedLoginAttempts = 0,
                LastLoginDate = null,
                PasswordLastChangedDate = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "User created successfully: {UserId} - {Email}",
                user.UserId,
                user.Email);

            return new UserResponse
            {
                UserId = user.UserId,
                Email = user.Email,
                MobileNumber = user.MobileNumber,
                IsEmailVerified = user.IsEmailVerified,
                IsMobileVerified = user.IsMobileVerified,
                IsActive = user.IsActive,
                IsLocked = user.IsLocked,
                FailedLoginAttempts = user.FailedLoginAttempts,
                LastLoginDate = user.LastLoginDate,
                PasswordLastChangedDate = user.PasswordLastChangedDate,
                CreatedDate = user.CreatedDate,
                UpdatedDate = user.UpdatedDate
            };
        }

        public async Task<UserResponse?> GetUserByIdAsync(int userId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (user == null)
                return null;

            return new UserResponse
            {
                UserId = user.UserId,
                Email = user.Email,
                MobileNumber = user.MobileNumber,
                IsEmailVerified = user.IsEmailVerified,
                IsMobileVerified = user.IsMobileVerified,
                IsActive = user.IsActive,
                IsLocked = user.IsLocked,
                FailedLoginAttempts = user.FailedLoginAttempts,
                LastLoginDate = user.LastLoginDate,
                PasswordLastChangedDate = user.PasswordLastChangedDate,
                CreatedDate = user.CreatedDate,
                UpdatedDate = user.UpdatedDate
            };
        }

        public async Task<PagedResponse<UserResponse>> GetUserListAsync(GetUserListRequest request)
        {
            var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
            var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

            var query = _context.Users.AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim().ToLower();

                query = query.Where(x =>
                    x.Email.ToLower().Contains(search) ||
                    (x.MobileNumber != null && x.MobileNumber.ToLower().Contains(search)));
            }

            // Active Filter
            if (request.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == request.IsActive);
            }

            var totalCount = await query.CountAsync();

            var users = await query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new UserResponse
                {
                    UserId = x.UserId,
                    Email = x.Email,
                    MobileNumber = x.MobileNumber,
                    IsEmailVerified = x.IsEmailVerified,
                    IsMobileVerified = x.IsMobileVerified,
                    IsActive = x.IsActive,
                    IsLocked = x.IsLocked,
                    FailedLoginAttempts = x.FailedLoginAttempts,
                    LastLoginDate = x.LastLoginDate,
                    PasswordLastChangedDate = x.PasswordLastChangedDate,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                })
                .ToListAsync();

            return new PagedResponse<UserResponse>
            {
                Items = users,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}