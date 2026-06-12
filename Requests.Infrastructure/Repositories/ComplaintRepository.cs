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

        public async Task<IList<ComplaintRequest>> GetAllAsync(int? employeeId, bool? isSeenByHR, string? recipientUserId, string? recipientRole, int page, int pageSize)
        {
            var query = _context.ComplaintRequests.AsQueryable();

            if (employeeId.HasValue)
                query = query.Where(c => c.EmployeeId == employeeId.Value);

            if (isSeenByHR.HasValue)
                query = query.Where(c => c.IsSeenByHR == isSeenByHR.Value);

            if (!string.IsNullOrEmpty(recipientUserId))
                query = query.Where(c => c.RecipientUserId == recipientUserId);

            if (!string.IsNullOrEmpty(recipientRole))
                query = query.Where(c => c.RecipientRole == recipientRole);

            return await query
                .OrderByDescending(c => c.Date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync(int? employeeId, bool? isSeenByHR, string? recipientUserId)
        {
            var query = _context.ComplaintRequests.AsQueryable();

            if (employeeId.HasValue)
                query = query.Where(c => c.EmployeeId == employeeId.Value);

            if (isSeenByHR.HasValue)
                query = query.Where(c => c.IsSeenByHR == isSeenByHR.Value);

            if (!string.IsNullOrEmpty(recipientUserId))
                query = query.Where(c => c.RecipientUserId == recipientUserId);

            return await query.CountAsync();
        }

        public async Task<int> GetUnseenCountAsync(string? recipientUserId, string role)
        {
            var query = _context.ComplaintRequests.AsQueryable();

            if (role == "HR")
                return await query.CountAsync(c => !c.IsSeenByHR);
            else if (role == "AreaManager" && !string.IsNullOrEmpty(recipientUserId))
                return await query.CountAsync(c => c.RecipientUserId == recipientUserId && !c.IsSeenByAreaManager);
            else if (role == "CEO")
                return await query.CountAsync(c => c.RecipientRole == "CEO" && !c.IsSeenByCEO);
            else if (role == "Employee")
                return await query.CountAsync(c => c.RecipientRole == "Employee" && !c.IsSeenByEmployee);

            return 0;
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