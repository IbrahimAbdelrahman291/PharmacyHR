using System;
using System.Collections.Generic;
using System.Text;

namespace Payroll.Application.DTOs
{
    public class BorrowItemDto
    {
        public int Id { get; set; }
        public double Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime DateOfBorrow { get; set; }
    }
}
