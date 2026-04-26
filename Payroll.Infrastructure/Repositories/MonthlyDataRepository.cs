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
                .Include(x => x.Discounts)
                .Include(x => x.ContractDiscounts)
                .Include(x => x.Bonuses)
                .Include(x => x.CashBorrows)
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

        public async Task AddDiscountAsync(int employeeId, double amount, string reason, string? notes)
        {
            var data = await GetCurrentAsync(employeeId);
            if (data is null) return;

            var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            var egyptNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);

            var discount = new Discount
            {
                MonthlyEmployeeDataId = data.Id,
                Amount = amount,
                ReasonOfDiscount = reason,
                Notes = notes,
                Year = egyptNow.Year,
                Month = egyptNow.Month,
                Date = egyptNow
            };

            await _context.Discounts.AddAsync(discount);
            data.TotalDiscounts = (data.TotalDiscounts ?? 0) + amount;
            await RecalculateNetSalaryAsync(data);
            await _context.SaveChangesAsync();
        }

        public async Task AddContractDiscountAsync(int employeeId, double amount, string reason, string? notes)
        {
            var data = await GetCurrentAsync(employeeId);
            if (data is null) return;

            var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            var egyptNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);

            var contractDiscount = new ContractDiscount
            {
                MonthlyEmployeeDataId = data.Id,
                Amount = amount,
                ReasonOfDiscount = reason,
                Notes = notes,
                Year = egyptNow.Year,
                Month = egyptNow.Month,
                Date = egyptNow
            };

            await _context.ContractDiscounts.AddAsync(contractDiscount);
            data.TotalContractDiscount = (data.TotalContractDiscount ?? 0) + amount;
            await RecalculateNetSalaryAsync(data);
            await _context.SaveChangesAsync();
        }

        public async Task AddBonusAsync(int employeeId, double amount, string reason, string? notes)
        {
            var data = await GetCurrentAsync(employeeId);
            if (data is null) return;

            var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            var egyptNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);

            var bonus = new Bonus
            {
                MonthlyEmployeeDataId = data.Id,
                Amount = amount,
                Reason = reason,
                Notes = notes,
                Year = egyptNow.Year,
                Month = egyptNow.Month,
                DateOfBonus = egyptNow
            };

            await _context.Bonuses.AddAsync(bonus);
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

        public async Task AddCashBorrowAsync(int employeeId, double amount, string reason, string? notes)
        {
            var data = await GetCurrentAsync(employeeId);
            if (data is null) return;

            var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            var egyptNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);

            var cashBorrow = new CashBorrow
            {
                MonthlyEmployeeDataId = data.Id,
                Amount = amount,
                Reason = reason,
                Notes = notes,
                Year = egyptNow.Year,
                Month = egyptNow.Month,
                DateOfBorrow = egyptNow
            };

            await _context.CashBorrows.AddAsync(cashBorrow);
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
                .Include(x => x.Discounts)
                .Include(x => x.ContractDiscounts)
                .Include(x => x.Bonuses)
                .Include(x => x.CashBorrows)
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

        public async Task<bool> DeleteDiscountAsync(int id)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount is null) return false;

            var data = await _context.MonthlyEmployeeData.FindAsync(discount.MonthlyEmployeeDataId);
            if (data is not null)
            {
                data.TotalDiscounts = (data.TotalDiscounts ?? 0) - discount.Amount;
                await RecalculateNetSalaryAsync(data);
            }

            _context.Discounts.Remove(discount);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteContractDiscountAsync(int id)
        {
            var discount = await _context.ContractDiscounts.FindAsync(id);
            if (discount is null) return false;

            var data = await _context.MonthlyEmployeeData.FindAsync(discount.MonthlyEmployeeDataId);
            if (data is not null)
            {
                data.TotalContractDiscount = (data.TotalContractDiscount ?? 0) - discount.Amount;
                await RecalculateNetSalaryAsync(data);
            }

            _context.ContractDiscounts.Remove(discount);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteBonusAsync(int id)
        {
            var bonus = await _context.Bonuses.FindAsync(id);
            if (bonus is null) return false;

            var data = await _context.MonthlyEmployeeData.FindAsync(bonus.MonthlyEmployeeDataId);
            if (data is not null)
            {
                data.TotalBouns = (data.TotalBouns ?? 0) - bonus.Amount;
                await RecalculateNetSalaryAsync(data);
            }

            _context.Bonuses.Remove(bonus);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCashBorrowAsync(int id)
        {
            var cashBorrow = await _context.CashBorrows.FindAsync(id);
            if (cashBorrow is null) return false;

            var data = await _context.MonthlyEmployeeData.FindAsync(cashBorrow.MonthlyEmployeeDataId);
            if (data is not null)
            {
                data.TotalCashBorrows = (data.TotalCashBorrows ?? 0) - cashBorrow.Amount;
                await RecalculateNetSalaryAsync(data);
            }

            _context.CashBorrows.Remove(cashBorrow);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task BulkAddDiscountAsync(IList<int> employeeIds, double amount, string reason, string? notes)
        {
            var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            var egyptNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);

            var monthlyData = await _context.MonthlyEmployeeData
                .Where(x => employeeIds.Contains(x.EmployeeId)
                    && x.Month == egyptNow.Month
                    && x.Year == egyptNow.Year)
                .ToListAsync();

            var discounts = monthlyData.Select(data => new Discount
            {
                MonthlyEmployeeDataId = data.Id,
                Amount = amount,
                ReasonOfDiscount = reason,
                Notes = notes,
                Year = egyptNow.Year,
                Month = egyptNow.Month,
                Date = egyptNow
            }).ToList();

            await _context.Discounts.AddRangeAsync(discounts);

            foreach (var data in monthlyData)
            {
                data.TotalDiscounts = (data.TotalDiscounts ?? 0) + amount;
                await RecalculateNetSalaryAsync(data);
            }

            await _context.SaveChangesAsync();
        }

        public async Task BulkAddBonusAsync(IList<int> employeeIds, double amount, string reason, string? notes)
        {
            var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            var egyptNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);

            var monthlyData = await _context.MonthlyEmployeeData
                .Where(x => employeeIds.Contains(x.EmployeeId)
                    && x.Month == egyptNow.Month
                    && x.Year == egyptNow.Year)
                .ToListAsync();

            var bonuses = monthlyData.Select(data => new Bonus
            {
                MonthlyEmployeeDataId = data.Id,
                Amount = amount,
                Reason = reason,
                Notes = notes,
                Year = egyptNow.Year,
                Month = egyptNow.Month,
                DateOfBonus = egyptNow
            }).ToList();

            await _context.Bonuses.AddRangeAsync(bonuses);

            foreach (var data in monthlyData)
            {
                data.TotalBouns = (data.TotalBouns ?? 0) + amount;
                await RecalculateNetSalaryAsync(data);
            }

            await _context.SaveChangesAsync();
        }
        public async Task<IList<MonthlyEmployeeData>> GetAllByMonthAndYearAsync(int month, int year, int? branchId)
        {
            var query = _context.MonthlyEmployeeData
                .Where(x => x.Month == month && x.Year == year);

            if (branchId.HasValue)
                query = query.Where(x => x.EmployeeId == branchId.Value);

            return await query.ToListAsync();
        }
    }
}