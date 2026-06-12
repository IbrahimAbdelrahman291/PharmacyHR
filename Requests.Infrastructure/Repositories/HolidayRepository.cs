using Microsoft.EntityFrameworkCore;
using Requests.Domain.Entities;
using Requests.Domain.Interfaces;
using Requests.Infrastructure.Data;

namespace Requests.Infrastructure.Repositories
{
    public class HolidayRepository : IHolidayRepository
    {
        private readonly RequestsDbContext _context;

        public HolidayRepository(RequestsDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddAsync(HolidayRequest request)
        {
            await _context.HolidayRequests.AddAsync(request);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IList<HolidayRequest>> GetAllAsync(int? employeeId, bool? isSeenByHR, string? areaManagerUserId, int page, int pageSize)
        {
            var query = _context.HolidayRequests.AsQueryable();

            if (employeeId.HasValue)
                query = query.Where(r => r.EmployeeId == employeeId.Value);

            if (isSeenByHR.HasValue)
                query = query.Where(r => r.IsSeenByHR == isSeenByHR.Value);

            if (!string.IsNullOrEmpty(areaManagerUserId))
                query = query.Where(r => r.AreaManagerUserId == areaManagerUserId);

            return await query
                .OrderByDescending(r => r.RequestDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync(int? employeeId, bool? isSeenByHR, string? areaManagerUserId)
        {
            var query = _context.HolidayRequests.AsQueryable();

            if (employeeId.HasValue)
                query = query.Where(r => r.EmployeeId == employeeId.Value);

            if (isSeenByHR.HasValue)
                query = query.Where(r => r.IsSeenByHR == isSeenByHR.Value);

            if (!string.IsNullOrEmpty(areaManagerUserId))
                query = query.Where(r => r.AreaManagerUserId == areaManagerUserId);

            return await query.CountAsync();
        }

        public async Task<HolidayRequest?> GetByIdAsync(int id)
            => await _context.HolidayRequests.FindAsync(id);

        public async Task<bool> UpdateAsync(HolidayRequest request)
        {
            _context.HolidayRequests.Update(request);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetUnseenCountAsync(string role) 
        {
            if (role == "HR")
            {
                return await _context.HolidayRequests.CountAsync(r => !r.IsSeenByHR);
            }
            else if (role == "Employee")
            {
                return await _context.HolidayRequests.CountAsync(r => r.IsSeenByEmployee == null);
            }
            return 0;
        }
    }

}
