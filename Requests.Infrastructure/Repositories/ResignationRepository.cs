using Microsoft.EntityFrameworkCore;
using Requests.Domain.Entities;
using Requests.Domain.Interfaces;
using Requests.Infrastructure.Data;

namespace Requests.Infrastructure.Repositories
{
    public class ResignationRepository : IResignationRepository
    {
        private readonly RequestsDbContext _context;

        public ResignationRepository(RequestsDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddAsync(ResignationRequest request)
        {
            await _context.ResignationRequests.AddAsync(request);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IList<ResignationRequest>> GetAllAsync(int? employeeId, bool? isSeenByHR, int page, int pageSize)
        {
            var query = _context.ResignationRequests.AsQueryable();
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
            var query = _context.ResignationRequests.AsQueryable();
            if (employeeId.HasValue)
                query = query.Where(r => r.EmployeeId == employeeId.Value);
            if (isSeenByHR.HasValue)
                query = query.Where(r => r.IsSeenByHR == isSeenByHR.Value);
            return await query.CountAsync();
        }

        public async Task<ResignationRequest?> GetByIdAsync(int id)
            => await _context.ResignationRequests.FindAsync(id);

        public async Task<bool> UpdateAsync(ResignationRequest request)
        {
            _context.ResignationRequests.Update(request);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetUnseenCountAsync(string role)
        {
            if (role == "HR")
                return await _context.ResignationRequests.CountAsync(r => !r.IsSeenByHR);
            else if (role == "Employee")
                return await _context.ResignationRequests.CountAsync(r => !r.IsSeenByEmployee);
            return 0;
        }
    }
}