using Identity.Application.DTOs;
using SharedKernel.Wrappers;


namespace Identity.Application.Interfaces
{
    public interface IAuthService
    {
        Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto request);
        Task<Result<bool>> CreateUserAsync(CreateUserDto dto);
        Task<Result<bool>> ChangePasswordAsync(string userId, ChangePasswordDto dto);
        Task<Result<PaginatedResponse<UserDto>>> GetAllUsersAsync(int page, int pageSize,string? name);
        Task<Result<bool>> ToggleUserAsync(string userId);
        Task<Result<IList<int>>> GetAreaManagerBranchesAsync(string userId);
        Task<Result<bool>> AddAreaManagerBranchAsync(string userId, int branchId);
        Task<Result<bool>> RemoveAreaManagerBranchAsync(string userId, int branchId);
    }
}
