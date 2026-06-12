
using Microsoft.EntityFrameworkCore;
using Requests.Domain.Entities;
using Requests.Domain.Interfaces;
using Requests.Infrastructure.Data;

namespace Requests.Infrastructure.Repositories
{
    public class ForgetedHoursRepository : IForgetedHoursRepository
    {
        private readonly RequestsDbContext _context;

        public ForgetedHoursRepository(RequestsDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddAsync(ForgetedHoursRequest request)
        {
            await _context.ForgetedHoursRequests.AddAsync(request);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IList<ForgetedHoursRequest>> GetAllAsync(int? employeeId, bool? isSeenByHR, int page, int pageSize)
        {
            var query = _context.ForgetedHoursRequests.AsQueryable();

            if (employeeId.HasValue)
                query = query.Where(r => r.EmployeeId == employeeId.Value);

            if (isSeenByHR.HasValue)
                query = query.Where(r => r.IsSeenByHR == isSeenByHR.Value);

            return await query
                .OrderByDescending(r => r.RequestDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync(int? employeeId, bool? isSeenByHR)
        {
            var query = _context.ForgetedHoursRequests.AsQueryable();

            if (employeeId.HasValue)
                query = query.Where(r => r.EmployeeId == employeeId.Value);

            if (isSeenByHR.HasValue)
                query = query.Where(r => r.IsSeenByHR == isSeenByHR.Value);

            return await query.CountAsync();
        }

        public async Task<ForgetedHoursRequest?> GetByIdAsync(int id)
            => await _context.ForgetedHoursRequests.FindAsync(id);

        public async Task<bool> UpdateAsync(ForgetedHoursRequest request)
        {
            _context.ForgetedHoursRequests.Update(request);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetUnseenCountAsync(string role)
        {
            if (role == "HR")
                return await _context.ForgetedHoursRequests.CountAsync(r => !r.IsSeenByHR);
            if (role == "Employee")
                return await _context.ForgetedHoursRequests.CountAsync(r => !r.IsSeenByEmployee);
            return 0;
        }

        public async Task<int> GetMonthlyCountAsync(int employeeId, int month, int year)
            => await _context.ForgetedHoursRequests
                .CountAsync(r => r.EmployeeId == employeeId
                    && r.RequestDate.Month == month
                    && r.RequestDate.Year == year);
    }
}
