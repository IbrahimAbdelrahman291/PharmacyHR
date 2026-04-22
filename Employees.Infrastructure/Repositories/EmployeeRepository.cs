using Employees.Domain.Entities;
using Employees.Domain.Interfaces;
using Employees.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace Employees.Infrastructure.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly EmployeesDbContext _context;

        public EmployeeRepository(EmployeesDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddAsync(Employee employee, EmployeeHistory history)
        {
            await _context.Employees.AddAsync(employee);
            await _context.SaveChangesAsync();

            history.EmployeeId = employee.Id;
            await _context.EmployeeHistories.AddAsync(history);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Employee?> GetByIdAsync(int id)
            => await _context.Employees.FindAsync(id);

        public async Task<IList<Employee>> GetAllAsync(int page, int pageSize, int? branchId)
        {
            var query = _context.Employees.AsQueryable();
            if (branchId.HasValue)
                query = query.Where(e => e.BranchId == branchId);

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync(int? branchId)
        {
            var query = _context.Employees.AsQueryable();
            if (branchId.HasValue)
                query = query.Where(e => e.BranchId == branchId);
            return await query.CountAsync();
        }

        public async Task<bool> UpdateAsync(Employee employee)
        {
            _context.Employees.Update(employee);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
