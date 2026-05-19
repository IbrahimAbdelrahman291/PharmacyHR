using Attendance.Domain.Entities;
using Attendance.Domain.Interfaces;
using Attendance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace Attendance.Infrastructure.Repositories
{
    public class WorkLogRepository : IWorkLogRepository
    {
        private readonly AttendanceDbContext _context;

        public WorkLogRepository(AttendanceDbContext context)
        {
            _context = context;
        }

        public async Task<WorkLog?> GetOpenShiftAsync(int employeeId)
        {
            var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            var egyptNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);
            var egyptDate = DateOnly.FromDateTime(egyptNow);

            // بيدور على شيفت مفتوح النهارده
            var workLog = await _context.WorkLogs
                .FirstOrDefaultAsync(w => w.EmployeeId == employeeId
                    && w.Day == egyptDate
                    && w.End == TimeOnly.MinValue);

            // لو مش لاقي → بيدور على شيفت مفتوح امبارح (Night Shift)
            if (workLog is null)
            {
                var previousDate = egyptDate.AddDays(-1);
                workLog = await _context.WorkLogs
                    .FirstOrDefaultAsync(w => w.EmployeeId == employeeId
                        && w.Day == previousDate
                        && w.End == TimeOnly.MinValue);
            }

            return workLog;
        }

        public async Task<WorkLog?> GetOpenShiftByDateAsync(int employeeId, DateOnly date)
            => await _context.WorkLogs
                .FirstOrDefaultAsync(w => w.EmployeeId == employeeId
                    && w.Day == date
                    && w.End == TimeOnly.MinValue);

        public async Task<bool> HasShiftOnDayAsync(int employeeId, DateOnly date)
            => await _context.WorkLogs
                .AnyAsync(w => w.EmployeeId == employeeId && w.Day == date);

        public async Task<bool> AddAsync(WorkLog workLog)
        {
            await _context.WorkLogs.AddAsync(workLog);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateAsync(WorkLog workLog)
        {
            _context.WorkLogs.Update(workLog);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IList<WorkLog>> GetAllAsync(int employeeId, int page, int pageSize)
            => await _context.WorkLogs
                .Where(w => w.EmployeeId == employeeId)
                .OrderByDescending(w => w.Day)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

        public async Task<int> GetTotalCountAsync(int employeeId)
            => await _context.WorkLogs.CountAsync(w => w.EmployeeId == employeeId);
        public async Task<IList<WorkLog>> GetReportAsync(DateOnly fromDate, DateOnly toDate, int? employeeId, int? branchId)
        {
            var query = _context.WorkLogs
                .Where(w => w.Day >= fromDate && w.Day <= toDate);

            if (employeeId.HasValue)
                query = query.Where(w => w.EmployeeId == employeeId.Value);

            return await query.OrderByDescending(w => w.Day).ToListAsync();
        }
    }
}
