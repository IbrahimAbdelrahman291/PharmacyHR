using System;
using System.Collections.Generic;
using System.Text;

namespace Payroll.Application.DTOs
{
    public class MonthlyDataDto
    {
        public int EmployeeId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
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
        public double? TotalCashBorrows { get; set; }
        public double? totalInstallmentBorrow { get; set; }
        public int? Holidaies { get; set; }
        public double? NetSalary { get; set; }
        public IList<DiscountItemDto> Discounts { get; set; } = new List<DiscountItemDto>();
        public IList<DiscountItemDto> ContractDiscounts { get; set; } = new List<DiscountItemDto>();
        public IList<BonusItemDto> Bonuses { get; set; } = new List<BonusItemDto>();
        public IList<BorrowItemDto> CashBorrows { get; set; } = new List<BorrowItemDto>();
    }
}
