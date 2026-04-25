using Payroll.Application.Interfaces;
using Payroll.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace Payroll.Infrastructure.Jobs
{
    public class NewMonthJob : INewMonthJob
    {
        private readonly PayrollDbContext _context;

        public NewMonthJob(PayrollDbContext context)
        {
            _context = context;
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
                    Month = egyptNow.Month,
                    Year = egyptNow.Year,
                    Target = data.Target,
                    Insurence = data.Insurence,
                    Holidaies = (previousMonth.Year < egyptNow.Year) ? 7 : data.Holidaies,
                    Hours = 0,
                    HoursOverTime = 0,
                    ForgetedHours = 0,
                    HolidayHours = 0,
                    TotalDiscounts = 0,
                    TotalContractDiscount = 0,
                    TotalBouns = 0,
                    TotalBorrows = 0,
                    TotalCashBorrows = 0,
                    TotalSalary = data.TotalSalary,
                    SalaryPerHour = data.SalaryPerHour
                };

                await _context.MonthlyEmployeeData.AddAsync(newData);
            }

            await _context.SaveChangesAsync();
        }
    }
}
