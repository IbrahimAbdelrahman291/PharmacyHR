using System;
using System.Collections.Generic;
using System.Text;

namespace Employees.Application.DTOs
{
    public class EmployeeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? theNameOfJob { get; set; }
        public string? BankName { get; set; }
        public string? BankAccount { get; set; }
        public TimeOnly? CheckInTime { get; set; }
        public TimeOnly? CheckOutTime { get; set; }
        public double? ShiftHours { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
    }
}
