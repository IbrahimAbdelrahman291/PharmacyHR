using System;
using System.Collections.Generic;
using System.Text;

namespace Employees.Application.DTOs
{
    public class UpdateEmployeeDto
    {
        public string? Name { get; set; }
        public string? theNameOfJob { get; set; }
        public int? BankId { get; set; }
        public string? BankAccount { get; set; }
        public double? ShiftHours { get; set; }
        public int? BranchId { get; set; }
        public string? EmployeeType { get; set; }
        public string? Status { get; set; }
    }
}
