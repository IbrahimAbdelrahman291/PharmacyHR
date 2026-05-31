using Microsoft.EntityFrameworkCore;
using Requests.Domain.Entities;
using Requests.Domain.Interfaces;
using Requests.Infrastructure.Data;

namespace Requests.Infrastructure.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly RequestsDbContext _context;

        public AppointmentRepository(RequestsDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddAsync(AppointmentRequest request)
        {
            await _context.AppointmentRequests.AddAsync(request);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IList<AppointmentRequest>> GetAllAsync(bool? isSeenByHR, int page, int pageSize)
        {
            var query = _context.AppointmentRequests.AsQueryable();
            if (isSeenByHR.HasValue)
                query = query.Where(r => r.IsSeenByHR == isSeenByHR.Value);
            return await query
                .OrderByDescending(r => r.RequestDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync(bool? isSeenByHR)
        {
            var query = _context.AppointmentRequests.AsQueryable();
            if (isSeenByHR.HasValue)
                query = query.Where(r => r.IsSeenByHR == isSeenByHR.Value);
            return await query.CountAsync();
        }

        public async Task<AppointmentRequest?> GetByIdAsync(int id)
            => await _context.AppointmentRequests.FindAsync(id);

        public async Task<bool> UpdateAsync(AppointmentRequest request)
        {
            _context.AppointmentRequests.Update(request);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetUnseenCountAsync()
            => await _context.AppointmentRequests.CountAsync(r => !r.IsSeenByHR);
    }
}