using Microsoft.EntityFrameworkCore;
using Requests.Domain.Entities;
using Requests.Domain.Interfaces;
using Requests.Infrastructure.Data;

namespace Requests.Infrastructure.Repositories
{
    public class BorrowRepository : IBorrowRepository
    {
        private readonly RequestsDbContext _context;

        public BorrowRepository(RequestsDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddBorrowRequestAsync(BorrowRequest request)
        {
            await _context.BorrowRequests.AddAsync(request);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IList<BorrowRequest>> GetAllBorrowRequestsAsync(int? employeeId, bool? isSeenByHR, int page, int pageSize)
        {
            var query = _context.BorrowRequests.AsQueryable();

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

        public async Task<int> GetTotalBorrowRequestsCountAsync(int? employeeId, bool? isSeenByHR)
        {
            var query = _context.BorrowRequests.AsQueryable();

            if (employeeId.HasValue)
                query = query.Where(r => r.EmployeeId == employeeId.Value);

            if (isSeenByHR.HasValue)
                query = query.Where(r => r.IsSeenByHR == isSeenByHR.Value);

            return await query.CountAsync();
        }

        public async Task<BorrowRequest?> GetBorrowRequestByIdAsync(int id)
            => await _context.BorrowRequests.FindAsync(id);

        public async Task<bool> UpdateBorrowRequestAsync(BorrowRequest request)
        {
            _context.BorrowRequests.Update(request);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetUnseenBorrowCountAsync(string role)
        {
            if (role == "HR")
            {
                return await _context.BorrowRequests.CountAsync(r => !r.IsSeenByHR);
            }
            else if (role == "Employee")
            {
                return await _context.BorrowRequests.CountAsync(r => !r.IsSeenByEmployee);
            }
            return 0;
        }

        public async Task<bool> AddInstallmentBorrowAsync(InstallmentBorrow borrow)
        {
            await _context.InstallmentBorrows.AddAsync(borrow);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IList<InstallmentBorrow>> GetActiveInstallmentBorrowsAsync()
            => await _context.InstallmentBorrows
                .Where(b => b.IsActive && b.RemainingMonths > 0)
                .ToListAsync();

        public async Task<IList<InstallmentBorrow>> GetInstallmentBorrowsByEmployeeAsync(int employeeId)
            => await _context.InstallmentBorrows
                .Where(b => b.EmployeeId == employeeId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

        public async Task<bool> UpdateInstallmentBorrowAsync(InstallmentBorrow borrow)
        {
            _context.InstallmentBorrows.Update(borrow);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}