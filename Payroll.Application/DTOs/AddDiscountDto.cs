using System;
using System.Collections.Generic;
using System.Text;

namespace Payroll.Application.DTOs
{
    public class AddDiscountDto
    {
        public int EmployeeId { get; set; }
        public double Amount { get; set; }
        public string ReasonOfDiscount { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
