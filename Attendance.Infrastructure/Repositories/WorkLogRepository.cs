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

            var workLog = await _context.WorkLogs
                .FirstOrDefaultAsync(w => w.EmployeeId == employeeId
                    && w.Day == egyptDate
                    && w.IsEnd == false
                    );

            if (workLog is null)
            {
                var previousDate = egyptDate.AddDays(-1);
                workLog = await _context.WorkLogs
                    .FirstOrDefaultAsync(w => w.EmployeeId == employeeId
                        && w.Day == previousDate
                        && w.IsEnd == false
                        );

            }

            return workLog;
        }

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

        public async Task<IList<WorkLog>> GetReportAsync(DateOnly fromDate, DateOnly toDate, int? employeeId)
        {
            var query = _context.WorkLogs
                .Where(w => w.Day >= fromDate && w.Day <= toDate);

            if (employeeId.HasValue)
                query = query.Where(w => w.EmployeeId == employeeId.Value);

            return await query.OrderByDescending(w => w.Day).ToListAsync();
        }

        public async Task<int> GetReportCountAsync(DateOnly fromDate, DateOnly toDate, int? employeeId)
        {
            var query = _context.WorkLogs
                .Where(w => w.Day >= fromDate && w.Day <= toDate);

            if (employeeId.HasValue)
                query = query.Where(w => w.EmployeeId == employeeId.Value);

            return await query.CountAsync();
        }
    }
}