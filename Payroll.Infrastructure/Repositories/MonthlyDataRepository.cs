using Microsoft.EntityFrameworkCore;
using Payroll.Domain.Entities;
using Payroll.Infrastructure.Data;
using SharedKernel.Interfaces;

namespace Payroll.Infrastructure.Repositories
{
    public class MonthlyDataRepository : Payroll.Domain.Interfaces.IMonthlyDataRepository, SharedKernel.Interfaces.IMonthlyDataRepository
    {
        private readonly PayrollDbContext _context;
        private readonly IEmployeeRepository _employeeRepository;

        public MonthlyDataRepository(PayrollDbContext context, IEmployeeRepository employeeRepository)
        {
            _context = context;
            _employeeRepository = employeeRepository;
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

        public async Task AddHoursAsync(int employeeId, double hours)
        {
            var data = await GetCurrentAsync(employeeId);
            if (data is null) return;
            data.Hours = (data.Hours ?? 0) + hours;
            await RecalculateNetSalaryAsync(data);
            await _context.SaveChangesAsync();
        }

        public async Task AddHolidayHoursAsync(int employeeId, double hours, int TotalDays)
        {
            var data = await GetCurrentAsync(employeeId);
            if (data is null) return;
            data.HolidayHours = (data.HolidayHours ?? 0) + hours;
            data.Holidaies = (data.Holidaies ?? 0) - TotalDays;
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
            data.HoursOverTime = (data.HoursOverTime ?? 0) + hours;
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
            var activeEmployeeIds = await _employeeRepository.GetActiveEmployeeIdsAsync();

            var query = _context.MonthlyEmployeeData
                .Where(x => x.Month == month
                    && x.Year == year
                    && activeEmployeeIds.Contains(x.EmployeeId));

            if (branchId.HasValue)
                query = query.Where(x => x.BranchId == branchId.Value);

            return await query.ToListAsync();
        }
        public async Task CreateMonthlyDataAsync(int employeeId, string role, double? totalSalary, double? salaryPerHour, double target, int branchId,double? insurence, int Holidaies)
        {
            var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            var egyptNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);

            var data = new MonthlyEmployeeData
            {
                EmployeeId = employeeId,
                Role = role.ToLower(),
                BranchId = branchId,
                Month = egyptNow.Month,
                Year = egyptNow.Year,
                Target = target,
                Holidaies = Holidaies,
                Hours = 0,
                HoursOverTime = 0,
                ForgetedHours = 0,
                HolidayHours = 0,
                TotalDiscounts = 0,
                TotalContractDiscount = 0,
                TotalBouns = 0,
                TotalBorrows = 0,
                TotalCashBorrows = 0,
                Insurence = insurence
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
        public async Task<IList<Discount>> GetDiscountsAsync(int employeeId, int? month, int? year)
        {
            var query = _context.MonthlyEmployeeData
                .Where(x => x.EmployeeId == employeeId);

            if (month.HasValue && year.HasValue)
                query = query.Where(x => x.Month == month && x.Year == year);

            var monthlyDataIds = await query.Select(x => x.Id).ToListAsync();

            return await _context.Discounts
                .Where(d => monthlyDataIds.Contains(d.MonthlyEmployeeDataId))
                .ToListAsync();
        }

        public async Task<IList<ContractDiscount>> GetContractDiscountsAsync(int employeeId, int? month, int? year)
        {
            var query = _context.MonthlyEmployeeData
                .Where(x => x.EmployeeId == employeeId);

            if (month.HasValue && year.HasValue)
                query = query.Where(x => x.Month == month && x.Year == year);

            var monthlyDataIds = await query.Select(x => x.Id).ToListAsync();

            return await _context.ContractDiscounts
                .Where(d => monthlyDataIds.Contains(d.MonthlyEmployeeDataId))
                .ToListAsync();
        }

        public async Task<IList<Bonus>> GetBonusesAsync(int employeeId, int? month, int? year)
        {
            var query = _context.MonthlyEmployeeData
                .Where(x => x.EmployeeId == employeeId);

            if (month.HasValue && year.HasValue)
                query = query.Where(x => x.Month == month && x.Year == year);

            var monthlyDataIds = await query.Select(x => x.Id).ToListAsync();

            return await _context.Bonuses
                .Where(b => monthlyDataIds.Contains(b.MonthlyEmployeeDataId))
                .ToListAsync();
        }

        public async Task<IList<CashBorrow>> GetCashBorrowsAsync(int employeeId, int? month, int? year)
        {
            var query = _context.MonthlyEmployeeData
                .Where(x => x.EmployeeId == employeeId);

            if (month.HasValue && year.HasValue)
                query = query.Where(x => x.Month == month && x.Year == year);

            var monthlyDataIds = await query.Select(x => x.Id).ToListAsync();

            return await _context.CashBorrows
                .Where(c => monthlyDataIds.Contains(c.MonthlyEmployeeDataId))
                .ToListAsync();
        }

        public async Task<int?> GetHolidaysInCurrentMonthAsync(int employeeId)
        {
            var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            var egyptNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);

            var Holiydays = await _context.MonthlyEmployeeData
                .Where(x => x.EmployeeId == employeeId)
                .Select(x => x.Holidaies)
                .FirstOrDefaultAsync();

            return Holiydays;

        }

        public async Task<double?> GetTotalSalaryForInstallmentBorrow(int employeeId)
        {

            var employeeRole = _employeeRepository.GetRoleName(employeeId);

            var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            var egyptNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);

            double? totalSalary = 0;
            double? Quarter = 0;
            double? ShiftHours = 0;
            double? SalaryPerHour = 0;
            if (employeeRole is not null)
            {
                if (employeeRole == "static")
                {
                    totalSalary = await _context.MonthlyEmployeeData
                        .Where(x => x.EmployeeId == employeeId)
                        .Select(x => x.TotalSalary)
                        .FirstOrDefaultAsync();
                    Quarter = totalSalary / 4;
                }
                else if (employeeRole == "changable")
                {
                    ShiftHours = await _context.MonthlyEmployeeData.Where(x => x.EmployeeId == employeeId).Select(x => x.Target).FirstOrDefaultAsync();;
                    SalaryPerHour = await _context.MonthlyEmployeeData.Where(y => y.EmployeeId == employeeId).Select(y => y.SalaryPerHour).FirstOrDefaultAsync();

                    totalSalary = (ShiftHours * SalaryPerHour) / 26;
                    Quarter = totalSalary / 4;
                }
                else if (employeeRole == "delivery") 
                {
                    ShiftHours = await _context.MonthlyEmployeeData.Where(x => x.EmployeeId == employeeId).Select(x => x.Target).FirstOrDefaultAsync(); ;
                    SalaryPerHour = await _context.MonthlyEmployeeData.Where(y => y.EmployeeId == employeeId).Select(y => y.SalaryPerHour).FirstOrDefaultAsync();

                    totalSalary = (ShiftHours * SalaryPerHour);
                    Quarter = totalSalary / 4;
                }
            }

            

            return Quarter;
        }
    }
}