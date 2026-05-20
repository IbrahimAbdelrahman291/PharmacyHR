using Attendance.Application.DTOs;
using SharedKernel.Wrappers;

namespace Attendance.Application.Interfaces
{
    public interface IAttendanceService
    {
        Task<Result<bool>> StartShiftAsync(int employeeId);
        Task<Result<bool>> EndShiftAsync(int employeeId);
        Task<Result<PaginatedResponse<AttendanceReportDto>>> GetReportAsync(string type, DateOnly fromDate, DateOnly toDate, int? employeeId, int? branchId, int page, int pageSize);
        Task<Result<PaginatedResponse<AbsentReportDto>>> GetAbsentReportAsync(DateOnly fromDate, DateOnly toDate, int? branchId, int page, int pageSize);
        Task<Result<PaginatedResponse<WorkLogDto>>> GetMyShiftsAsync(int employeeId, DateOnly fromDate, DateOnly toDate, int page, int pageSize);
    }
}