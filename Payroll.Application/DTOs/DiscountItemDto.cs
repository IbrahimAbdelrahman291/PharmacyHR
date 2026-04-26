using System;
using System.Collections.Generic;
using System.Text;

namespace Payroll.Application.DTOs
{
    public class DiscountItemDto
    {
        public int Id { get; set; }
        public double Amount { get; set; }
        public string ReasonOfDiscount { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime Date { get; set; }
    }
}
