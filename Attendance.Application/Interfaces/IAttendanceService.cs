using Attendance.Application.DTOs;
using SharedKernel.Wrappers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Attendance.Application.Interfaces
{
    public interface IAttendanceService
    {
        Task<Result<bool>> StartShiftAsync(int employeeId);
        Task<Result<bool>> EndShiftAsync(int employeeId);
        Task<Result<PaginatedResponse<WorkLogDto>>> GetAllAsync(int employeeId, int page, int pageSize);
        Task<Result<WorkLogDto>> GetOpenShiftAsync(int employeeId);
    }
}
