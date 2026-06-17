using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Domain.Enums;

namespace Identity.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var result = await _authService.LoginAsync(request);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(result.Value);
        }

        [HttpPost("CreateUser")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            var result = await _authService.CreateUserAsync(dto);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(new { message = "User created successfully" });
        }

        [HttpPut("users/{userId}/password")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> ChangePassword(string userId, [FromBody] ChangePasswordDto dto)
        {
            var result = await _authService.ChangePasswordAsync(userId, dto);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(new { message = "Password changed successfully" });
        }

        [HttpGet("users")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> GetAllUsers([FromQuery] string? name,[FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _authService.GetAllUsersAsync(page, pageSize,name);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(result.Value);
        }
        [HttpPut("users/{userId}/toggle")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> ToggleUser(string userId)
        {
            var result = await _authService.ToggleUserAsync(userId);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });
            return Ok(new { message = "User status updated successfully" });
        }
        [HttpGet("users/{userId}/branches")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> GetAreaManagerBranches(string userId)
        {
            var result = await _authService.GetAreaManagerBranchesAsync(userId);
            return Ok(result.Value);
        }

        [HttpPost("users/{userId}/branches/{branchId}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> AddAreaManagerBranch(string userId, int branchId)
        {
            var result = await _authService.AddAreaManagerBranchAsync(userId, branchId);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });
            return Ok(new { message = "Branch added successfully" });
        }

        [HttpDelete("users/{userId}/branches/{branchId}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> RemoveAreaManagerBranch(string userId, int branchId)
        {
            var result = await _authService.RemoveAreaManagerBranchAsync(userId, branchId);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });
            return Ok(new { message = "Branch removed successfully" });
        }
    }
}