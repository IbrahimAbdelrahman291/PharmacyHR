using System;
using System.Collections.Generic;
using System.Text;

namespace Payroll.Application.DTOs
{
    public class BulkDiscountDto
    {
        public IList<int> EmployeeIds { get; set; } = new List<int>();
        public double Amount { get; set; }
        public string ReasonOfDiscount { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
