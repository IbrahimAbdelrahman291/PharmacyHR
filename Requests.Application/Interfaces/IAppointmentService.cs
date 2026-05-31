using Requests.Application.DTOs;
using SharedKernel.Wrappers;

namespace Requests.Application.Interfaces
{
    public interface IAppointmentService
    {
        Task<Result<bool>> AddAsync(string areaManagerUserId, CreateAppointmentRequestDto dto);
        Task<Result<PaginatedResponse<AppointmentRequestDto>>> GetAllAsync(bool? isSeenByHR, int page, int pageSize);
        Task<Result<bool>> ApproveOrRejectAsync(int id, ApproveRejectDto dto);
        Task<Result<int>> GetUnseenCountAsync();
        Task<Result<bool>> MarkAsSeenAsync(int id);
    }
}