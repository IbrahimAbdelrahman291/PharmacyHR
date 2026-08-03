using Microsoft.EntityFrameworkCore;
using Payroll.Domain.Entities;
using Payroll.Infrastructure.Data;
using SharedKernel.Interfaces;
using SharedKernel.Wrappers;

namespace Payroll.Infrastructure.Repositories
{
    public class MonthlyDataRepository : Payroll.Domain.Interfaces.IMonthlyDataRepository, SharedKernel.Interfaces.IMonthlyDataRepository
    {
        private readonly PayrollDbContext _context;
        private readonly IEmployeeRepository _employeeRepository;
        private const int MaxRetries = 3;

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

        // Method جديدة: بتاخد شهر/سنة اختياريين، لو موجودين تستخدم GetByMonthAndYearAsync، لو لأ ترجع لـ GetCurrentAsync
        // من غير ما تلمس GetCurrentAsync نفسها خالص
        private async Task<MonthlyEmployeeData?> GetTargetMonthDataAsync(int employeeId, int? month, int? year)
        {
            if (month.HasValue && year.HasValue)
                return await GetByMonthAndYearAsync(employeeId, month.Value, year.Value);

            return await GetCurrentAsync(employeeId);
        }

        private async Task RecalculateNetSalaryAsync(MonthlyEmployeeData data)
        {
            var totalBorrows = (data.TotalBorrows ?? 0) + (data.TotalCashBorrows ?? 0) + (data.totalInstallmentBorrow ?? 0);
            var totalDiscounts = (data.TotalDiscounts ?? 0) + (data.TotalContractDiscount ?? 0);
            var insurance = data.Insurence ?? 0;
            var totalBonus = (data.TotalBouns ?? 0);
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

        // ===================================================================
        // Helper 1: للـ mutation البسيطة اللي بتعدل في MonthlyEmployeeData بس
        // ===================================================================
        private async Task<Result<bool>> ApplyMutationAsync(int employeeId, Action<MonthlyEmployeeData> mutation)
        {
            for (int attempt = 0; attempt < MaxRetries; attempt++)
            {
                try
                {
                    var data = await GetCurrentAsync(employeeId);
                    if (data is null)
                        return Result<bool>.Failure($"لا يوجد سجل بيانات شهرية للموظف {employeeId} لهذا الشهر");

                    mutation(data);
                    await RecalculateNetSalaryAsync(data);
                    await _context.SaveChangesAsync();
                    return Result<bool>.Success(true);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (attempt == MaxRetries - 1)
                        return Result<bool>.Failure("فشل التحديث بسبب تعارض متزامن، حاول مرة أخرى خلال عشر دقائق");

                    foreach (var entry in _context.ChangeTracker.Entries())
                        entry.State = EntityState.Detached;
                }
            }
            return Result<bool>.Failure("فشل غير متوقع أثناء تحديث البيانات هناك تعديل على نفس البيانات في نفس الوقت خلال عشر دقائق");
        }

        // ==========================================================================
        // Helper 2: للـ mutation + إضافة سجل تفصيلي (Discount/Bonus/CashBorrow)
        // دلوقتي بتقبل month/year اختياريين عشان الـ HR يقدر يحدد شهر مختلف عن الحالي
        // ==========================================================================
        private async Task<Result<bool>> ApplyMutationWithDetailRecordAsync(
            int employeeId,
            Action<MonthlyEmployeeData> mutation,
            Func<MonthlyEmployeeData, object> createDetailRecord,
            int? month = null,
            int? year = null)
        {
            for (int attempt = 0; attempt < MaxRetries; attempt++)
            {
                try
                {
                    var data = await GetTargetMonthDataAsync(employeeId, month, year);
                    if (data is null)
                    {
                        var monthLabel = month.HasValue && year.HasValue ? $"{month}/{year}" : "الشهر الحالي";
                        return Result<bool>.Failure($"لا يوجد سجل بيانات شهرية للموظف {employeeId} لـ {monthLabel}");
                    }

                    var detailRecord = createDetailRecord(data);
                    await _context.AddAsync(detailRecord);

                    mutation(data);
                    await RecalculateNetSalaryAsync(data);
                    await _context.SaveChangesAsync();
                    return Result<bool>.Success(true);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (attempt == MaxRetries - 1)
                        return Result<bool>.Failure("فشل التحديث بسبب تعارض متزامن، حاول مرة أخرى");

                    // لازم نشيل الـ detail record اللي اتضاف في المحاولة اللي فشلت
                    // عشان مايتكررش في المحاولة الجاية
                    foreach (var entry in _context.ChangeTracker.Entries().ToList())
                        entry.State = EntityState.Detached;
                }
            }
            return Result<bool>.Failure("فشل غير متوقع أثناء تحديث البيانات");
        }

        // ================= المجموعة 1: Simple mutations =================

        public async Task<Result<bool>> AddHoursAsync(int employeeId, double hours)
            => await ApplyMutationAsync(employeeId, data =>
                data.Hours = (data.Hours ?? 0) + hours);

        public async Task<Result<bool>> AddHolidayHoursAsync(int employeeId, double hours, int TotalDays)
            => await ApplyMutationAsync(employeeId, data =>
            {
                data.HolidayHours = (data.HolidayHours ?? 0) + hours;
                data.Holidaies = (data.Holidaies) - TotalDays;
            });

        public async Task<Result<bool>> AddForgetedHoursAsync(int employeeId, double hours)
            => await ApplyMutationAsync(employeeId, data =>
                data.ForgetedHours = (data.ForgetedHours ?? 0) + hours);

        public async Task<Result<bool>> UpdateHoursOverTimeAsync(int employeeId, double hours)
            => await ApplyMutationAsync(employeeId, data =>
                data.HoursOverTime = (data.HoursOverTime ?? 0) + hours);

        public async Task<Result<bool>> AddBorrowAsync(int employeeId, double amount)
            => await ApplyMutationAsync(employeeId, data =>
                data.TotalBorrows = (data.TotalBorrows ?? 0) + amount);

        public async Task<Result<bool>> UpdateInstallmentBorrow(int employeeId, double amount)
            => await ApplyMutationAsync(employeeId, data =>
                data.totalInstallmentBorrow = amount);

        public async Task<Result<bool>> UpdateSalaryAsync(int employeeId, double totalSalary)
            => await ApplyMutationAsync(employeeId, data =>
                data.TotalSalary = totalSalary);

        public async Task<Result<bool>> UpdateSalaryPerHourAsync(int employeeId, double salaryPerHour)
            => await ApplyMutationAsync(employeeId, data =>
                data.SalaryPerHour = salaryPerHour);

        public async Task<Result<bool>> UpdateInsurenceAsync(int employeeId, double amount)
            => await ApplyMutationAsync(employeeId, data =>
                data.Insurence = amount);

        public async Task<Result<bool>> UpdateHolidaysHours(int employeeId, double HolidayHours)
            => await ApplyMutationAsync(employeeId, data =>
                data.HolidayHours = HolidayHours);

        public async Task<Result<bool>> UpdateHolidays(int employeeId, int Holidays)
            => await ApplyMutationAsync(employeeId, data =>
                data.Holidaies = Holidays);

        // ================= المجموعة 2: Mutation + Detail Record =================
        // كل واحدة دلوقتي بتقبل month/year اختياريين (default null = الشهر الحالي)

        public async Task<Result<bool>> AddDiscountAsync(int employeeId, double amount, string reason, string? notes, int? month = null, int? year = null)
            => await ApplyMutationWithDetailRecordAsync(
                employeeId,
                mutation: data => data.TotalDiscounts = (data.TotalDiscounts ?? 0) + amount,
                createDetailRecord: data =>
                {
                    var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
                    var egyptNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);
                    return new Discount
                    {
                        MonthlyEmployeeDataId = data.Id,
                        Amount = amount,
                        ReasonOfDiscount = reason,
                        Notes = notes,
                        Year = data.Year,      // شهر/سنة الـ MonthlyEmployeeData المستهدفة، مش وقت التنفيذ
                        Month = data.Month,
                        Date = egyptNow        // وقت التنفيذ الفعلي (إمتى الـ HR عمل العملية)
                    };
                },
                month: month,
                year: year);

        public async Task<Result<bool>> AddContractDiscountAsync(int employeeId, double amount, string reason, string? notes, int? month = null, int? year = null)
            => await ApplyMutationWithDetailRecordAsync(
                employeeId,
                mutation: data => data.TotalContractDiscount = (data.TotalContractDiscount ?? 0) + amount,
                createDetailRecord: data =>
                {
                    var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
                    var egyptNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);
                    return new ContractDiscount
                    {
                        MonthlyEmployeeDataId = data.Id,
                        Amount = amount,
                        ReasonOfDiscount = reason,
                        Notes = notes,
                        Year = data.Year,
                        Month = data.Month,
                        Date = egyptNow
                    };
                },
                month: month,
                year: year);

        public async Task<Result<bool>> AddBonusAsync(int employeeId, double amount, string reason, string? notes, int? month = null, int? year = null)
            => await ApplyMutationWithDetailRecordAsync(
                employeeId,
                mutation: data => data.TotalBouns = (data.TotalBouns ?? 0) + amount,
                createDetailRecord: data =>
                {
                    var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
                    var egyptNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);
                    return new Bonus
                    {
                        MonthlyEmployeeDataId = data.Id,
                        Amount = amount,
                        Reason = reason,
                        Notes = notes,
                        Year = data.Year,
                        Month = data.Month,
                        DateOfBonus = egyptNow
                    };
                },
                month: month,
                year: year);

        public async Task<Result<bool>> AddCashBorrowAsync(int employeeId, double amount, string reason, string? notes, int? month = null, int? year = null)
            => await ApplyMutationWithDetailRecordAsync(
                employeeId,
                mutation: data => data.TotalCashBorrows = (data.TotalCashBorrows ?? 0) + amount,
                createDetailRecord: data =>
                {
                    var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
                    var egyptNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);
                    return new CashBorrow
                    {
                        MonthlyEmployeeDataId = data.Id,
                        Amount = amount,
                        Reason = reason,
                        Notes = notes,
                        Year = data.Year,
                        Month = data.Month,
                        DateOfBorrow = egyptNow
                    };
                },
                month: month,
                year: year);

        // ================= المجموعة 3: Delete methods (مع Retry) =================

        public async Task<Result<bool>> DeleteDiscountAsync(int id)
        {
            for (int attempt = 0; attempt < MaxRetries; attempt++)
            {
                try
                {
                    var discount = await _context.Discounts.FindAsync(id);
                    if (discount is null)
                        return Result<bool>.Failure("Discount not found");

                    var data = await _context.MonthlyEmployeeData.FindAsync(discount.MonthlyEmployeeDataId);
                    if (data is not null)
                    {
                        data.TotalDiscounts = (data.TotalDiscounts ?? 0) - discount.Amount;
                        await RecalculateNetSalaryAsync(data);
                    }

                    _context.Discounts.Remove(discount);
                    await _context.SaveChangesAsync();
                    return Result<bool>.Success(true);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (attempt == MaxRetries - 1)
                        return Result<bool>.Failure("فشل الحذف بسبب تعارض متزامن، حاول مرة أخرى");

                    foreach (var entry in _context.ChangeTracker.Entries())
                        entry.State = EntityState.Detached;
                }
            }
            return Result<bool>.Failure("فشل غير متوقع أثناء الحذف");
        }

        public async Task<Result<bool>> DeleteContractDiscountAsync(int id)
        {
            for (int attempt = 0; attempt < MaxRetries; attempt++)
            {
                try
                {
                    var discount = await _context.ContractDiscounts.FindAsync(id);
                    if (discount is null)
                        return Result<bool>.Failure("Contract discount not found");

                    var data = await _context.MonthlyEmployeeData.FindAsync(discount.MonthlyEmployeeDataId);
                    if (data is not null)
                    {
                        data.TotalContractDiscount = (data.TotalContractDiscount ?? 0) - discount.Amount;
                        await RecalculateNetSalaryAsync(data);
                    }

                    _context.ContractDiscounts.Remove(discount);
                    await _context.SaveChangesAsync();
                    return Result<bool>.Success(true);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (attempt == MaxRetries - 1)
                        return Result<bool>.Failure("فشل الحذف بسبب تعارض متزامن، حاول مرة أخرى");

                    foreach (var entry in _context.ChangeTracker.Entries())
                        entry.State = EntityState.Detached;
                }
            }
            return Result<bool>.Failure("فشل غير متوقع أثناء الحذف");
        }

        public async Task<Result<bool>> DeleteBonusAsync(int id)
        {
            for (int attempt = 0; attempt < MaxRetries; attempt++)
            {
                try
                {
                    var bonus = await _context.Bonuses.FindAsync(id);
                    if (bonus is null)
                        return Result<bool>.Failure("Bonus not found");

                    var data = await _context.MonthlyEmployeeData.FindAsync(bonus.MonthlyEmployeeDataId);
                    if (data is not null)
                    {
                        data.TotalBouns = (data.TotalBouns ?? 0) - bonus.Amount;
                        await RecalculateNetSalaryAsync(data);
                    }

                    _context.Bonuses.Remove(bonus);
                    await _context.SaveChangesAsync();
                    return Result<bool>.Success(true);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (attempt == MaxRetries - 1)
                        return Result<bool>.Failure("فشل الحذف بسبب تعارض متزامن، حاول مرة أخرى");

                    foreach (var entry in _context.ChangeTracker.Entries())
                        entry.State = EntityState.Detached;
                }
            }
            return Result<bool>.Failure("فشل غير متوقع أثناء الحذف");
        }

        public async Task<Result<bool>> DeleteCashBorrowAsync(int id)
        {
            for (int attempt = 0; attempt < MaxRetries; attempt++)
            {
                try
                {
                    var cashBorrow = await _context.CashBorrows.FindAsync(id);
                    if (cashBorrow is null)
                        return Result<bool>.Failure("Cash borrow not found");

                    var data = await _context.MonthlyEmployeeData.FindAsync(cashBorrow.MonthlyEmployeeDataId);
                    if (data is not null)
                    {
                        data.TotalCashBorrows = (data.TotalCashBorrows ?? 0) - cashBorrow.Amount;
                        await RecalculateNetSalaryAsync(data);
                    }

                    _context.CashBorrows.Remove(cashBorrow);
                    await _context.SaveChangesAsync();
                    return Result<bool>.Success(true);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (attempt == MaxRetries - 1)
                        return Result<bool>.Failure("فشل الحذف بسبب تعارض متزامن، حاول مرة أخرى");

                    foreach (var entry in _context.ChangeTracker.Entries())
                        entry.State = EntityState.Detached;
                }
            }
            return Result<bool>.Failure("فشل غير متوقع أثناء الحذف");
        }

        // ================= Query methods (بدون تغيير) =================

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

        // ================= Bulk methods (هنراجعها بعدين لوحدها) =================

        public async Task<Result<bool>> BulkAddDiscountAsync(IList<int> employeeIds, double amount, string reason, string? notes)
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
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> BulkAddBonusAsync(IList<int> employeeIds, double amount, string reason, string? notes)
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
            return Result<bool>.Success(true);
        }

        // ================= باقي الـ methods (بدون تغيير) =================

        public async Task<IList<MonthlyEmployeeData>> GetAllByMonthAndYearAsync(int month, int year, int? branchId, int page, int pageSize)
        {
            var activeEmployeeIds = await _employeeRepository.GetActiveEmployeeIdsAsync();

            var query = _context.MonthlyEmployeeData
                .Where(x => x.Month == month
                    && x.Year == year
                    && activeEmployeeIds.Contains(x.EmployeeId));

            if (branchId.HasValue)
                query = query.Where(x => x.BranchId == branchId.Value);

            return await query
                .OrderByDescending(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task CreateMonthlyDataAsync(int employeeId, string role, double? totalSalary, double? salaryPerHour, double target, int branchId, double? insurence, int Holidaies)
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
                .Where(x => x.EmployeeId == employeeId && x.Month == egyptNow.Month && x.Year == egyptNow.Year)
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
                        .Where(x => x.EmployeeId == employeeId
                         && x.Month == egyptNow.Month
                         && x.Year == egyptNow.Year)
                        .Select(x => x.TotalSalary)
                        .FirstOrDefaultAsync();
                    Quarter = totalSalary / 4;
                }
                else if (employeeRole == "changable")
                {
                    ShiftHours = await _context.MonthlyEmployeeData.Where(x => x.EmployeeId == employeeId && x.Month == egyptNow.Month && x.Year == egyptNow.Year).Select(x => x.Target).FirstOrDefaultAsync();
                    SalaryPerHour = await _context.MonthlyEmployeeData.Where(y => y.EmployeeId == employeeId && y.Month == egyptNow.Month && y.Year == egyptNow.Year).Select(y => y.SalaryPerHour).FirstOrDefaultAsync();

                    totalSalary = (ShiftHours * SalaryPerHour) / 26;
                    Quarter = totalSalary / 4;
                }
                else if (employeeRole == "delivery")
                {
                    ShiftHours = await _context.MonthlyEmployeeData.Where(x => x.EmployeeId == employeeId && x.Month == egyptNow.Month && x.Year == egyptNow.Year).Select(x => x.Target).FirstOrDefaultAsync();
                    SalaryPerHour = await _context.MonthlyEmployeeData.Where(y => y.EmployeeId == employeeId && y.Month == egyptNow.Month && y.Year == egyptNow.Year).Select(y => y.SalaryPerHour).FirstOrDefaultAsync();

                    totalSalary = (ShiftHours * SalaryPerHour);
                    Quarter = totalSalary / 4;
                }
            }

            return Quarter;
        }

        public async Task<int> GetTotalMonthlyDataCount(int? month, int? year, int? branchId)
        {
            var query = _context.MonthlyEmployeeData.AsQueryable();

            if (branchId.HasValue)
                query = query.Where(x => x.BranchId == branchId.Value);
            if (month.HasValue)
                query = query.Where(x => x.Month == month.Value);
            if (year.HasValue)
                query = query.Where(x => x.Year == year.Value);

            return await query.CountAsync();
        }
        // ===================================================================
        // Helper 3: للـ mutation اللي متأثرش على NetSalary خالص (زي Branch, Target)
        // ===================================================================
        private async Task<Result<bool>> ApplySimpleFieldMutationAsync(int employeeId, Action<MonthlyEmployeeData> mutation)
        {
            for (int attempt = 0; attempt < MaxRetries; attempt++)
            {
                try
                {
                    var data = await GetCurrentAsync(employeeId);
                    if (data is null)
                        return Result<bool>.Failure($"لا يوجد سجل بيانات شهرية للموظف {employeeId} لهذا الشهر");

                    mutation(data);
                    await _context.SaveChangesAsync();
                    return Result<bool>.Success(true);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (attempt == MaxRetries - 1)
                        return Result<bool>.Failure("فشل التحديث بسبب تعارض متزامن، حاول مرة أخرى");

                    foreach (var entry in _context.ChangeTracker.Entries())
                        entry.State = EntityState.Detached;
                }
            }
            return Result<bool>.Failure("فشل غير متوقع أثناء تحديث البيانات");
        }

        public async Task<Result<bool>> UpdateBranchAsync(int employeeId, int branchId)
            => await ApplySimpleFieldMutationAsync(employeeId, data =>
                data.BranchId = branchId);

        public async Task<Result<bool>> UpdateTargetAsync(int employeeId, double target)
            => await ApplySimpleFieldMutationAsync(employeeId, data =>
                data.Target = target);
        public async Task<(double? Target, int? BranchId)?> GetCurrentTargetAndBranchAsync(int employeeId)
        {
            var data = await GetCurrentAsync(employeeId);
            if (data is null) return null;

            return (data.Target, data.BranchId);
        }
    }
}