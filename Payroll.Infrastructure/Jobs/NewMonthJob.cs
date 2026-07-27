using Microsoft.EntityFrameworkCore;
using Payroll.Application.Interfaces;
using Payroll.Infrastructure.Data;
using SharedKernel.Interfaces;


namespace Payroll.Infrastructure.Jobs
{
    public class NewMonthJob : INewMonthJob
    {
        private readonly PayrollDbContext _context;
        private readonly IInstallmentBorrowJob _installmentBorrowJob;

        public NewMonthJob(PayrollDbContext context, IInstallmentBorrowJob installmentBorrowJob)
        {
            _context = context;
            _installmentBorrowJob = installmentBorrowJob;
        }

        public async Task ExecuteAsync()
        {
            var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            var egyptNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);
            var previousMonth = egyptNow.AddMonths(-1);

            var previousData = await _context.MonthlyEmployeeData
                .Where(x => x.Month == previousMonth.Month && x.Year == previousMonth.Year)
                .ToListAsync();

            foreach (var data in previousData)
            {
                var exists = await _context.MonthlyEmployeeData
                    .AnyAsync(x => x.EmployeeId == data.EmployeeId
                        && x.Month == egyptNow.Month
                        && x.Year == egyptNow.Year);

                if (exists) continue;

                var newData = new Payroll.Domain.Entities.MonthlyEmployeeData
                {
                    EmployeeId = data.EmployeeId,
                    Role = data.Role,
                    BranchId = data.BranchId,
                    Month = egyptNow.Month,
                    Year = egyptNow.Year,
                    Target = data.Target,
                    Insurence = data.Insurence,
                    Holidaies = (previousMonth.Year < egyptNow.Year) ? 14 : data.Holidaies,
                    Hours = 0,
                    HoursOverTime = 0,
                    ForgetedHours = 0,
                    HolidayHours = 0,
                    TotalDiscounts = 0,
                    TotalContractDiscount = 0,
                    TotalBouns = 0,
                    TotalBorrows = 0,
                    TotalCashBorrows = 0,
                    totalInstallmentBorrow = 0,
                    TotalSalary = data.Role == "static" ? (data.TotalSalary - data.Insurence ?? 0) : 0,
                    SalaryPerHour = data.Role != "static" ? data.SalaryPerHour : null,
                    NetSalary = data.Role == "static" ? data.TotalSalary : 0,
                    
                };

                await _context.MonthlyEmployeeData.AddAsync(newData);
            }

            await _context.SaveChangesAsync();
            await _installmentBorrowJob.ProcessAsync();
        }
    }
}
