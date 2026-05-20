using Attendance.Domain.Entities;

namespace Attendance.Domain.Interfaces
{
    public interface IWorkLogRepository
    {
        Task<WorkLog?> GetOpenShiftAsync(int employeeId);
        Task<bool> HasShiftOnDayAsync(int employeeId, DateOnly date);
        Task<bool> AddAsync(WorkLog workLog);
        Task<bool> UpdateAsync(WorkLog workLog);
        Task<IList<WorkLog>> GetReportAsync(DateOnly fromDate, DateOnly toDate, int? employeeId);
        Task<int> GetReportCountAsync(DateOnly fromDate, DateOnly toDate, int? employeeId);
    }
}