using System;
using System.Collections.Generic;
using System.Text;

namespace Employees.Application.DTOs
{
    public class UpdateEmployeeDto
    {
        public string? theNameOfJob { get; set; }
        public string? BankName { get; set; }
        public string? BankAccount { get; set; }
        public double? ShiftHours { get; set; }
        public int? BranchId { get; set; }
    }
}
