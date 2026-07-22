using System;
using System.Collections.Generic;
using System.Text;

namespace Payroll.Application.DTOs
{
    public class MonthlyDataWithEmployeeDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
        public double? Target { get; set; }
        public double? Insurence { get; set; }
        public double? Hours { get; set; }
        public double? HoursOverTime { get; set; }
        public double? ForgetedHours { get; set; }
        public double? HolidayHours { get; set; }
        public double? TotalSalary { get; set; }
        public double? TotalDiscounts { get; set; }
        public double? TotalContractDiscount { get; set; }
        public double? TotalBouns { get; set; }
        public double? TotalBorrows { get; set; }
        public double? TotalCashBorrows { get; set; }
        public double? TotalInstallmentsBorrow { get; set; }
        public double? NetSalary { get; set; }
        public string? BankName { get; set; }
        public string? BankAccount { get; set; }
        public double? SalaryPerHour { get; set; }
    }
}
