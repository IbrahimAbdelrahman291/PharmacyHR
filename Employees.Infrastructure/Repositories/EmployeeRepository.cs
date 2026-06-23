using Employees.Domain.Entities;
using Employees.Domain.Interfaces;
using Employees.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace Employees.Infrastructure.Repositories
{
    public class EmployeeRepository : IEmployeeRepository,
    SharedKernel.Interfaces.IEmployeeRepository,
    SharedKernel.Interfaces.IEmployeeScheduleRepository,
    SharedKernel.Interfaces.IEmployeeTypeRepository
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
                .OrderByDescending(e => e.Id)
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
        public async Task<IList<(int EmployeeId, TimeOnly CheckInTime, TimeOnly CheckOutTime)>> GetAllEmployeesWithScheduleByDayAsync(DayOfWeek dayOfWeek)
        {
            return await _context.EmployeeSchedules
                .Where(s => s.DayOfWeek == dayOfWeek)
                .Select(s => new ValueTuple<int, TimeOnly, TimeOnly>(s.EmployeeId, s.CheckInTime, s.CheckOutTime))
                .ToListAsync();
        }
        public async Task<bool> AddEmployeeBranchAsync(EmployeeBranch employeeBranch)
        {
            await _context.EmployeeBranches.AddAsync(employeeBranch);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IList<EmployeeBranch>> GetEmployeeBranchesAsync(int employeeId)
            => await _context.EmployeeBranches
                .Where(e => e.EmployeeId == employeeId)
                .OrderByDescending(e => e.StartDate)
                .ToListAsync();
        public async Task<bool> AddEvaluationAsync(QuarterlyEvaluation evaluation)
        {
            await _context.QuarterlyEvaluations.AddAsync(evaluation);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IList<QuarterlyEvaluation>> GetEvaluationsAsync(int employeeId)
            => await _context.QuarterlyEvaluations
                .Include(e => e.EvaluationResults)
                .Where(e => e.EmployeeId == employeeId)
                .OrderByDescending(e => e.Year)
                .ToListAsync();

        public async Task<QuarterlyEvaluation?> GetEvaluationByIdAsync(int id)
            => await _context.QuarterlyEvaluations
                .Include(e => e.EvaluationResults)
                .FirstOrDefaultAsync(e => e.Id == id);
        public async Task<QuarterlyEvaluation?> GetEvaluationByQuarterAsync(int employeeId, string quarter, int year)
            => await _context.QuarterlyEvaluations
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId
                    && e.Quarter == quarter
                    && e.Year == year);
        public async Task<EvaluationCriteria?> GetEvaluationCriteriaByIdAsync(int id)
            => await _context.EvaluationCriterias.FindAsync(id);

        public string? GetRoleName(int employeeId)
        {
            var employeeRole = _context.Employees.Where(x => x.Id == employeeId).Select(x => x.Role).FirstOrDefault();
            return employeeRole;
        }
        public async Task<bool> AddCustodyAsync(PersonalCustody custody)
        {
            await _context.PersonalCustodies.AddAsync(custody);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCustodyAsync(int id)
        {
            var custody = await _context.PersonalCustodies.FindAsync(id);
            if (custody is null) return false;
            _context.PersonalCustodies.Remove(custody);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IList<PersonalCustody>> GetCustodiesByEmployeeIdAsync(int employeeId)
            => await _context.PersonalCustodies
                .Where(c => c.EmployeeId == employeeId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

        public async Task UpdateEmployeeTypeAsync(int employeeId, string type)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee is null) return;
            employee.EmployeeType = type;
            await _context.SaveChangesAsync();
        }
        public async Task<IList<int>> GetActiveEmployeeIdsAsync()
            => await _context.Employees
                .Where(e => e.Status == "Active")
                .Select(e => e.Id)
                .ToListAsync();

        public async Task<bool> DeleteAsync(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee is null) return false;
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
