using Requests.Domain.Entities;

namespace Requests.Domain.Interfaces
{
    public interface IOvertimeRepository
    {
        Task<bool> AddAsync(OvertimeRequest request);
        Task<IList<OvertimeRequest>> GetAllAsync(int? employeeId, bool? isSeenByHR,string? userId, string role, int page, int pageSize);
        Task<int> GetTotalCountAsync(int? employeeId, bool? isSeenByHR, string? userId, string role);
        Task<OvertimeRequest?> GetByIdAsync(int id);
        Task<bool> UpdateAsync(OvertimeRequest request);
        Task<int> GetUnseenCountAsync(string? userId, string role, int? employeeId);
    }
}