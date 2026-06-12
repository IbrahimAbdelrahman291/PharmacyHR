using Requests.Application.DTOs;
using SharedKernel.Wrappers;

namespace Requests.Application.Interfaces
{
    public interface IForgetedHoursService
    {
        Task<Result<bool>> AddAsync(int employeeId, CreateForgetedHoursDto dto);
        Task<Result<PaginatedResponse<ForgetedHoursDto>>> GetAllAsync(int? employeeId, bool? isSeenByHR, int page, int pageSize);
        Task<Result<bool>> ApproveOrRejectAsync(int id, ApproveRejectDto dto);
        Task<Result<int>> GetUnseenCountAsync(string role);
        Task<Result<bool>> MarkAsSeenAsync(int id, string role);
    }
}
