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
        public int Month { get; set; }
        public int Year { get; set; }
        public double? TotalSalary { get; set; }
        public double? TotalDiscounts { get; set; }
        public double? TotalContractDiscount { get; set; }
        public double? TotalBouns { get; set; }
        public double? TotalBorrows { get; set; }
        public double? TotalCashBorrows { get; set; }
        public double? NetSalary { get; set; }
    }
}
