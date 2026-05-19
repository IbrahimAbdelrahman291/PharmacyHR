using Attendance.Domain.Entities;

namespace Attendance.Domain.Interfaces
{
    public interface IWorkLogRepository
    {
        Task<WorkLog?> GetOpenShiftAsync(int employeeId);
        Task<WorkLog?> GetOpenShiftByDateAsync(int employeeId, DateOnly date);
        Task<bool> HasShiftOnDayAsync(int employeeId, DateOnly date);
        Task<bool> AddAsync(WorkLog workLog);
        Task<bool> UpdateAsync(WorkLog workLog);
        Task<IList<WorkLog>> GetAllAsync(int employeeId, int page, int pageSize);
        Task<int> GetTotalCountAsync(int employeeId);
        Task<IList<WorkLog>> GetReportAsync(DateOnly fromDate, DateOnly toDate, int? employeeId, int? branchId);
    }
}
