using Microsoft.EntityFrameworkCore;
using Payroll.Domain.Entities;

namespace Payroll.Infrastructure.Data
{
    public class PayrollDbContext : DbContext
    {
        public PayrollDbContext(DbContextOptions<PayrollDbContext> options) : base(options) { }

        public DbSet<MonthlyEmployeeData> MonthlyEmployeeData { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<ContractDiscount> ContractDiscounts { get; set; }
        public DbSet<Bonus> Bonuses { get; set; }
        public DbSet<Borrow> Borrows { get; set; }
        public DbSet<CashBorrow> CashBorrows { get; set; }
    }
}
