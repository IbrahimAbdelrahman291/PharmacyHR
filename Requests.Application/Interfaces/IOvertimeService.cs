using Requests.Application.DTOs;
using SharedKernel.Wrappers;

namespace Requests.Application.Interfaces
{
    public interface IOvertimeService
    {
        Task<Result<bool>> AddAsync(int employeeId, CreateOvertimeRequestDto dto);
        Task<Result<PaginatedResponse<OvertimeRequestDto>>> GetAllAsync(int? employeeId, bool? isSeenByHR,string? userId, string role, int page, int pageSize);
        Task<Result<bool>> ControlApproveAsync(int id, string controlUserId, ApproveRejectDto dto);
        Task<Result<bool>> AreaManagerApproveAsync(int id, string areaManagerUserId, ApproveRejectDto dto);
        Task<Result<bool>> HRApproveAsync(int id, ApproveRejectDto dto);
        Task<Result<int>> GetUnseenCountAsync(string? userId, string role, int? employeeId);
        Task<Result<bool>> MarkAsSeenAsync(int id, string role);
    }
}