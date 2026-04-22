using Identity.Application.DTOs;
using SharedKernel.Wrappers;


namespace Identity.Application.Interfaces
{
    public interface IAuthService
    {
        Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto request);
        Task<Result<bool>> CreateUserAsync(CreateUserDto dto);
        Task<Result<bool>> ChangePasswordAsync(string userId, ChangePasswordDto dto);
        Task<Result<PaginatedResponse<UserDto>>> GetAllUsersAsync(int page, int pageSize);
    }
}
