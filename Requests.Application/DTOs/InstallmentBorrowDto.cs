using System;
using System.Collections.Generic;
using System.Text;

namespace Requests.Application.DTOs
{
    public class InstallmentBorrowDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public double TotalAmount { get; set; }
        public double MonthlyAmount { get; set; }
        public int TotalMonths { get; set; }
        public int RemainingMonths { get; set; }
        public int StartMonth { get; set; }
        public int StartYear { get; set; }
        public bool IsActive { get; set; }
        public bool HasResignation { get; set; }
    }
}
