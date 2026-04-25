using SharedKernel.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Payroll.Domain.Entities
{
    public class CashBorrow : BaseEntity
    {
        public int MonthlyEmployeeDataId { get; set; }
        public double Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public DateTime DateOfBorrow { get; set; }
    }
}
