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
        public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _authService.GetAllUsersAsync(page, pageSize);
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
    }
}