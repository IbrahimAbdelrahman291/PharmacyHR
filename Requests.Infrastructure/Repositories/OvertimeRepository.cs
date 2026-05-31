using Microsoft.EntityFrameworkCore;
using Requests.Domain.Entities;
using Requests.Domain.Interfaces;
using Requests.Infrastructure.Data;

namespace Requests.Infrastructure.Repositories
{
    public class OvertimeRepository : IOvertimeRepository
    {
        private readonly RequestsDbContext _context;

        public OvertimeRepository(RequestsDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddAsync(OvertimeRequest request)
        {
            await _context.OvertimeRequests.AddAsync(request);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IList<OvertimeRequest>> GetAllAsync(int? employeeId, string? userId, string role, int page, int pageSize)
        {
            var query = _context.OvertimeRequests.AsQueryable();

            if (employeeId.HasValue)
                query = query.Where(r => r.EmployeeId == employeeId.Value);

            if (role == "AreaManager" && !string.IsNullOrEmpty(userId))
                query = query.Where(r => r.AreaManagerUserId == userId);

            return await query
                .OrderByDescending(r => r.RequestDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync(int? employeeId, string? userId, string role)
        {
            var query = _context.OvertimeRequests.AsQueryable();

            if (employeeId.HasValue)
                query = query.Where(r => r.EmployeeId == employeeId.Value);

            if (role == "AreaManager" && !string.IsNullOrEmpty(userId))
                query = query.Where(r => r.AreaManagerUserId == userId);

            return await query.CountAsync();
        }

        public async Task<OvertimeRequest?> GetByIdAsync(int id)
            => await _context.OvertimeRequests.FindAsync(id);

        public async Task<bool> UpdateAsync(OvertimeRequest request)
        {
            _context.OvertimeRequests.Update(request);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetUnseenCountAsync(string? userId, string role)
        {
            if (role == "HR")
                return await _context.OvertimeRequests.CountAsync(r => !r.IsSeenByHR);
            else if (role == "Control")
                return await _context.OvertimeRequests.CountAsync(r => !r.IsSeenByControl);
            else if (role == "AreaManager" && !string.IsNullOrEmpty(userId))
                return await _context.OvertimeRequests.CountAsync(r => r.AreaManagerUserId == userId && !r.IsSeenByAreaManager);

            return 0;
        }
    }
}