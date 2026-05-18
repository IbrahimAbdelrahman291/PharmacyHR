using Employees.Domain.Entities;
using Employees.Domain.Interfaces;
using Employees.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace Employees.Infrastructure.Repositories
{
    public class EmployeeRepository : IEmployeeRepository,
    SharedKernel.Interfaces.IEmployeeRepository,
    SharedKernel.Interfaces.IEmployeeScheduleRepository
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

        public async Task<IList<Employee>> GetAllAsync(int page, int pageSize, int? branchId, int? bankId, string? role, string? name)
        {
            var query = _context.Employees.AsQueryable();

            if (branchId.HasValue)
                query = query.Where(e => e.BranchId == branchId.Value);

            if (bankId.HasValue)
                query = query.Where(e => e.BankId == bankId.Value);

            if (!string.IsNullOrEmpty(role))
                query = query.Where(e => e.Role.ToLower() == role.ToLower());

            if (!string.IsNullOrEmpty(name))
                query = query.Where(e => e.Name.Contains(name));

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync(int? branchId, int? bankId, string? role, string? name)
        {
            var query = _context.Employees.AsQueryable();

            if (branchId.HasValue)
                query = query.Where(e => e.BranchId == branchId.Value);

            if (bankId.HasValue)
                query = query.Where(e => e.BankId == bankId.Value);

            if (!string.IsNullOrEmpty(role))
                query = query.Where(e => e.Role.ToLower() == role.ToLower());

            if (!string.IsNullOrEmpty(name))
                query = query.Where(e => e.Name.Contains(name));

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
            var employee = await _context.Employees
                .Include(e => e.Bank)
                .FirstOrDefaultAsync(e => e.Id == employeeId);
            if (employee is null) return null;
            return (employee.Id, employee.Name, employee.BranchId, employee.Bank?.Name, employee.BankAccount);
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

        public async Task<double?> GetShiftHoursAsync(int employeeId)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            return employee?.ShiftHours;
        }
        public async Task<bool> AddBankAsync(Bank bank)
        {
            await _context.Banks.AddAsync(bank);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteBankAsync(int id)
        {
            var bank = await _context.Banks.FindAsync(id);
            if (bank is null) return false;
            _context.Banks.Remove(bank);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IList<Bank>> GetAllBanksAsync()
            => await _context.Banks.ToListAsync();

        public async Task<Bank?> GetBankByIdAsync(int id)
            => await _context.Banks.FindAsync(id);

        public async Task<(TimeOnly CheckInTime, TimeOnly CheckOutTime)?> GetEmployeeScheduleByDayAsync(int employeeId, DayOfWeek dayOfWeek)
        {
            var schedule = await _context.EmployeeSchedules
                .FirstOrDefaultAsync(s => s.EmployeeId == employeeId && s.DayOfWeek == dayOfWeek);
            if (schedule is null) return null;
            return (schedule.CheckInTime, schedule.CheckOutTime);
        }
    }
}
