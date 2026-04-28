using Employees.Domain.Entities;
using Employees.Domain.Interfaces;
using Employees.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace Employees.Infrastructure.Repositories
{
    public class EmployeeRepository : IEmployeeRepository, SharedKernel.Interfaces.IEmployeeRepository
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
        public async Task<EmployeeHistory?> GetHistoryByEmployeeIdAsync(int employeeId)
             => await _context.EmployeeHistories.FirstOrDefaultAsync(h => h.EmployeeId == employeeId);

        public async Task<bool> UpdateHistoryAsync(EmployeeHistory history)
        {
            _context.EmployeeHistories.Update(history);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<(int Id, string Name, int BranchId, string? BankName, string? BankAccount)?> GetEmployeeBasicInfoAsync(int employeeId)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee is null) return null;
            return (employee.Id, employee.Name, employee.BranchId, employee.BankName, employee.BankAccount);
        }

        public async Task<bool> AddScheduleAsync(EmployeeSchedule schedule)
        {
            await _context.EmployeeSchedules.AddAsync(schedule);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateScheduleAsync(EmployeeSchedule schedule)
        {
            _context.EmployeeSchedules.Update(schedule);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteScheduleAsync(int id)
        {
            var schedule = await _context.EmployeeSchedules.FindAsync(id);
            if (schedule is null) return false;
            _context.EmployeeSchedules.Remove(schedule);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IList<EmployeeSchedule>> GetSchedulesByEmployeeIdAsync(int employeeId)
            => await _context.EmployeeSchedules
                .Where(s => s.EmployeeId == employeeId)
                .ToListAsync();

        public async Task<EmployeeSchedule?> GetScheduleByDayAsync(int employeeId, DayOfWeek dayOfWeek)
            => await _context.EmployeeSchedules
                .FirstOrDefaultAsync(s => s.EmployeeId == employeeId && s.DayOfWeek == dayOfWeek);
    }
}
