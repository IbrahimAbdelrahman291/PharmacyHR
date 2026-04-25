using Microsoft.EntityFrameworkCore;
using Payroll.Domain.Entities;
using Payroll.Infrastructure.Data;

namespace Payroll.Infrastructure.Repositories
{
    public class MonthlyDataRepository : Payroll.Domain.Interfaces.IMonthlyDataRepository, SharedKernel.Interfaces.IMonthlyDataRepository
    {
        private readonly PayrollDbContext _context;

        public MonthlyDataRepository(PayrollDbContext context)
        {
            _context = context;
        }

        private async Task<MonthlyEmployeeData?> GetCurrentAsync(int employeeId)
        {
            var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            var egyptNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);

            return await _context.MonthlyEmployeeData
                .FirstOrDefaultAsync(x => x.EmployeeId == employeeId
                    && x.Month == egyptNow.Month
                    && x.Year == egyptNow.Year);
        }

        private async Task RecalculateNetSalaryAsync(MonthlyEmployeeData data)
        {
            var totalBorrows = (data.TotalBorrows ?? 0) + (data.TotalCashBorrows ?? 0);
            var totalDiscounts = (data.TotalDiscounts ?? 0) + (data.TotalContractDiscount ?? 0);
            var insurance = data.Insurence ?? 0;
            var totalBonus = data.TotalBouns ?? 0;
            var totalHours = (data.Hours ?? 0) + (data.HoursOverTime ?? 0)
                + (data.ForgetedHours ?? 0) + (data.HolidayHours ?? 0);

            if (data.Role == "static")
            {
                data.NetSalary = (data.TotalSalary ?? 0) + totalBonus
                    - (totalBorrows + totalDiscounts + insurance);
            }
            else if (data.Role == "changable")
            {
                data.TotalSalary = totalHours * (data.SalaryPerHour ?? 0) / 26;
                data.NetSalary = (data.TotalSalary ?? 0) + totalBonus
                    - (totalBorrows + totalDiscounts + insurance);
            }
            else if (data.Role == "delivery")
            {
                data.TotalSalary = totalHours * (data.SalaryPerHour ?? 0);
                data.NetSalary = (data.TotalSalary ?? 0) + totalBonus
                    - (totalBorrows + totalDiscounts + insurance);
            }
        }

        public async Task CreateMonthlyDataAsync(int employeeId, string role, double? totalSalary, double? salaryPerHour, double target)
        {
            var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            var egyptNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);

            var data = new MonthlyEmployeeData
            {
                EmployeeId = employeeId,
                Role = role.ToLower(),
                Month = egyptNow.Month,
                Year = egyptNow.Year,
                Target = target,
                Holidaies = 7,
                Hours = 0,
                HoursOverTime = 0,
                ForgetedHours = 0,
                HolidayHours = 0,
                TotalDiscounts = 0,
                TotalContractDiscount = 0,
                TotalBouns = 0,
                TotalBorrows = 0,
                TotalCashBorrows = 0,
                Insurence = 0
            };

            if (role.ToLower() == "static")
            {
                data.TotalSalary = totalSalary;
                data.NetSalary = totalSalary ?? 0;
            }
            else
            {
                data.SalaryPerHour = salaryPerHour;
                data.NetSalary = 0;
            }

            await _context.MonthlyEmployeeData.AddAsync(data);
            await _context.SaveChangesAsync();
        }

        public async Task AddHoursAsync(int employeeId, double hours)
        {
            var data = await GetCurrentAsync(employeeId);
            if (data is null) return;
            data.Hours = (data.Hours ?? 0) + hours;
            await RecalculateNetSalaryAsync(data);
            await _context.SaveChangesAsync();
        }

        public async Task AddHolidayHoursAsync(int employeeId, double hours)
        {
            var data = await GetCurrentAsync(employeeId);
            if (data is null) return;
            data.HolidayHours = (data.HolidayHours ?? 0) + hours;
            data.Holidaies = (data.Holidaies ?? 0) - 1;
            await RecalculateNetSalaryAsync(data);
            await _context.SaveChangesAsync();
        }

        public async Task AddForgetedHoursAsync(int employeeId, double hours)
        {
            var data = await GetCurrentAsync(employeeId);
            if (data is null) return;
            data.ForgetedHours = (data.ForgetedHours ?? 0) + hours;
            await RecalculateNetSalaryAsync(data);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateHoursOverTimeAsync(int employeeId, double hours)
        {
            var data = await GetCurrentAsync(employeeId);
            if (data is null) return;
            data.HoursOverTime = hours;
            await RecalculateNetSalaryAsync(data);
            await _context.SaveChangesAsync();
        }

        public async Task AddDiscountAsync(int employeeId, double amount)
        {
            var data = await GetCurrentAsync(employeeId);
            if (data is null) return;
            data.TotalDiscounts = (data.TotalDiscounts ?? 0) + amount;
            await RecalculateNetSalaryAsync(data);
            await _context.SaveChangesAsync();
        }

        public async Task AddContractDiscountAsync(int employeeId, double amount)
        {
            var data = await GetCurrentAsync(employeeId);
            if (data is null) return;
            data.TotalContractDiscount = (data.TotalContractDiscount ?? 0) + amount;
            await RecalculateNetSalaryAsync(data);
            await _context.SaveChangesAsync();
        }

        public async Task AddBonusAsync(int employeeId, double amount)
        {
            var data = await GetCurrentAsync(employeeId);
            if (data is null) return;
            data.TotalBouns = (data.TotalBouns ?? 0) + amount;
            await RecalculateNetSalaryAsync(data);
            await _context.SaveChangesAsync();
        }

        public async Task AddBorrowAsync(int employeeId, double amount)
        {
            var data = await GetCurrentAsync(employeeId);
            if (data is null) return;
            data.TotalBorrows = (data.TotalBorrows ?? 0) + amount;
            await RecalculateNetSalaryAsync(data);
            await _context.SaveChangesAsync();
        }

        public async Task AddCashBorrowAsync(int employeeId, double amount)
        {
            var data = await GetCurrentAsync(employeeId);
            if (data is null) return;
            data.TotalCashBorrows = (data.TotalCashBorrows ?? 0) + amount;
            await RecalculateNetSalaryAsync(data);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateSalaryAsync(int employeeId, double totalSalary)
        {
            var data = await GetCurrentAsync(employeeId);
            if (data is null) return;
            data.TotalSalary = totalSalary;
            await RecalculateNetSalaryAsync(data);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateSalaryPerHourAsync(int employeeId, double salaryPerHour)
        {
            var data = await GetCurrentAsync(employeeId);
            if (data is null) return;
            data.SalaryPerHour = salaryPerHour;
            await RecalculateNetSalaryAsync(data);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateInsurenceAsync(int employeeId, double amount)
        {
            var data = await GetCurrentAsync(employeeId);
            if (data is null) return;
            data.Insurence = amount;
            await RecalculateNetSalaryAsync(data);
            await _context.SaveChangesAsync();
        }

        public async Task<MonthlyEmployeeData?> GetCurrentMonthAsync(int employeeId)
            => await GetCurrentAsync(employeeId);

        public async Task<MonthlyEmployeeData?> GetByMonthAndYearAsync(int employeeId, int month, int year)
            => await _context.MonthlyEmployeeData
                .FirstOrDefaultAsync(x => x.EmployeeId == employeeId
                    && x.Month == month
                    && x.Year == year);

        public async Task<bool> AddAsync(MonthlyEmployeeData data)
        {
            await _context.MonthlyEmployeeData.AddAsync(data);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateAsync(MonthlyEmployeeData data)
        {
            _context.MonthlyEmployeeData.Update(data);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IList<MonthlyEmployeeData>> GetAllCurrentMonthAsync(int? branchId)
        {
            var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            var egyptNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);

            return await _context.MonthlyEmployeeData
                .Where(x => x.Month == egyptNow.Month && x.Year == egyptNow.Year)
                .ToListAsync();
        }
    }
}