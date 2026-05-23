using Microsoft.EntityFrameworkCore;
using Requests.Domain.Entities;
using Requests.Domain.Interfaces;
using Requests.Infrastructure.Data;

namespace Requests.Infrastructure.Repositories
{
    public class ComplaintRepository : IComplaintRepository
    {
        private readonly RequestsDbContext _context;

        public ComplaintRepository(RequestsDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddAsync(ComplaintRequest complaint)
        {
            await _context.ComplaintRequests.AddAsync(complaint);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IList<ComplaintRequest>> GetAllAsync(int? employeeId, bool? isSeenByHR, int page, int pageSize)
        {
            var query = _context.ComplaintRequests.AsQueryable();

            if (employeeId.HasValue)
                query = query.Where(c => c.EmployeeId == employeeId.Value);

            if (isSeenByHR.HasValue)
                query = query.Where(c => c.IsSeenByHR == isSeenByHR.Value);

            return await query
                .OrderByDescending(c => c.Date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync(int? employeeId, bool? isSeenByHR)
        {
            var query = _context.ComplaintRequests.AsQueryable();

            if (employeeId.HasValue)
                query = query.Where(c => c.EmployeeId == employeeId.Value);

            if (isSeenByHR.HasValue)
                query = query.Where(c => c.IsSeenByHR == isSeenByHR.Value);

            return await query.CountAsync();
        }

        public async Task<ComplaintRequest?> GetByIdAsync(int id)
            => await _context.ComplaintRequests.FindAsync(id);

        public async Task<bool> UpdateAsync(ComplaintRequest complaint)
        {
            _context.ComplaintRequests.Update(complaint);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetUnseenCountAsync()
            => await _context.ComplaintRequests.CountAsync(c => !c.IsSeenByHR);
    }
}