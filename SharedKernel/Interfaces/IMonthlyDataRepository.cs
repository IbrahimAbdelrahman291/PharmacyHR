using System;
using System.Collections.Generic;
using System.Text;

namespace SharedKernel.Interfaces
{
    public interface IMonthlyDataRepository
    {
        Task AddHoursAsync(int employeeId, double hours);
        Task AddHolidayHoursAsync(int employeeId, double hours);
        Task AddForgetedHoursAsync(int employeeId, double hours);
        Task UpdateHoursOverTimeAsync(int employeeId, double hours);
        Task AddDiscountAsync(int employeeId, double amount);
        Task AddContractDiscountAsync(int employeeId, double amount);
        Task AddBonusAsync(int employeeId, double amount);
        Task AddBorrowAsync(int employeeId, double amount);
        Task AddCashBorrowAsync(int employeeId, double amount);
        Task UpdateSalaryAsync(int employeeId, double totalSalary);
        Task UpdateSalaryPerHourAsync(int employeeId, double salaryPerHour);
        Task UpdateInsurenceAsync(int employeeId, double amount);
        Task CreateMonthlyDataAsync(int employeeId, string role, double? totalSalary, double? salaryPerHour, double target);

    }
}
