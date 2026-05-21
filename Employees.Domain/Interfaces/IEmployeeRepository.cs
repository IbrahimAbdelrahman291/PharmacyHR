using Employees.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Employees.Domain.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<bool> AddAsync(Employee employee, EmployeeHistory history);
        Task<Employee?> GetByIdAsync(int id);
        Task<IList<Employee>> GetAllAsync(int page, int pageSize, int? branchId, int? bankId, string? role, string? name);
        Task<int> GetTotalCountAsync(int? branchId, int? bankId, string? role, string? name);
        Task<bool> UpdateAsync(Employee employee);
        Task<EmployeeHistory?> GetHistoryByEmployeeIdAsync(int employeeId);
        Task<bool> UpdateHistoryAsync(EmployeeHistory history);
        Task<bool> AddScheduleAsync(EmployeeSchedule schedule);
        Task<bool> UpdateScheduleAsync(EmployeeSchedule schedule);
        Task<bool> DeleteScheduleAsync(int id);
        Task<IList<EmployeeSchedule>> GetSchedulesByEmployeeIdAsync(int employeeId);
        Task<EmployeeSchedule?> GetScheduleByDayAsync(int employeeId, DayOfWeek dayOfWeek);
        Task<bool> AddBankAsync(Bank bank);
        Task<bool> DeleteBankAsync(int id);
        Task<IList<Bank>> GetAllBanksAsync();
        Task<Bank?> GetBankByIdAsync(int id);
        Task<bool> AddEmployeeBranchAsync(EmployeeBranch employeeBranch);
        Task<IList<EmployeeBranch>> GetEmployeeBranchesAsync(int employeeId);

    }
}
