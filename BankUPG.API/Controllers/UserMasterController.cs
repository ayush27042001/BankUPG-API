using BankUPG.Application.Interfaces.UserMaster;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankUPG.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SuperAdmin")]
    public class UserMasterController : ControllerBase
    {
        private readonly IUserMasterService _userMasterService;
        private readonly ILogger<UserMasterController> _logger;

        public UserMasterController(
            IUserMasterService userMasterService,
            ILogger<UserMasterController> logger)
        {
            _userMasterService = userMasterService;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<UserResponse>>> CreateUser(
            [FromBody] CreateUserRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<UserResponse>
                    {
                        Success = false,
                        Message = "Invalid request data.",
                        Errors = ModelState.Values
                            .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                            .ToList()
                    });
                }

                var result = await _userMasterService.CreateUserAsync(request);

                return Ok(new ApiResponse<UserResponse>
                {
                    Success = true,
                    Message = "User created successfully.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");

                return BadRequest(new ApiResponse<UserResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<UserResponse>>> GetUserById(int userId)
        {
            try
            {
                var result = await _userMasterService.GetUserByIdAsync(userId);

                if (result == null)
                {
                    return NotFound(new ApiResponse<UserResponse>
                    {
                        Success = false,
                        Message = "User not found."
                    });
                }

                return Ok(new ApiResponse<UserResponse>
                {
                    Success = true,
                    Message = "User retrieved successfully.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user");

                return BadRequest(new ApiResponse<UserResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("list")]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<UserResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<PagedResponse<UserResponse>>>> GetUserList(
            [FromQuery] GetUserListRequest request)
        {
            try
            {
                var result = await _userMasterService.GetUserListAsync(request);

                return Ok(new ApiResponse<PagedResponse<UserResponse>>
                {
                    Success = true,
                    Message = "Users retrieved successfully.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");

                return BadRequest(new ApiResponse<PagedResponse<UserResponse>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}