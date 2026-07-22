using SharedKernel.Common;


namespace Payroll.Domain.Entities
{
    public class MonthlyEmployeeData : BaseEntity
    {
        public int EmployeeId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int BranchId { get; set; }
        public double? Hours { get; set; }
        public double? HoursOverTime { get; set; }
        public double? ForgetedHours { get; set; }
        public double? Target { get; set; }
        public double? Insurence { get; set; }
        public double? HolidayHours { get; set; }
        public double? SalaryPerHour { get; set; }
        public double? TotalSalary { get; set; }
        public double? TotalDiscounts { get; set; }
        public double? TotalContractDiscount { get; set; }
        public double? TotalBouns { get; set; }
        public double? TotalBorrows { get; set; }
        public double? totalInstallmentBorrow { get; set; }
        public double? TotalCashBorrows { get; set; }
        public int? Holidaies { get; set; }
        public double? NetSalary { get; set; }
        public string Role { get; set; } = string.Empty; // static, changable, delivery
        // navigation properties
        public ICollection<Discount> Discounts { get; set; } = new List<Discount>();
        public ICollection<ContractDiscount> ContractDiscounts { get; set; } = new List<ContractDiscount>();
        public ICollection<Bonus> Bonuses { get; set; } = new List<Bonus>();
        public ICollection<CashBorrow> CashBorrows { get; set; } = new List<CashBorrow>();

    }
}
