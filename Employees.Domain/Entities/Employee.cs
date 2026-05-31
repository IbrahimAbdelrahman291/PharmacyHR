using SharedKernel.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Employees.Domain.Entities
{
    public class Employee : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // static, changable, delivery
        public string? theNameOfJob { get; set; }
        public int? BankId { get; set; }
        public string? BankAccount { get; set; }
        public double? ShiftHours { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int BranchId { get; set; }
        public Bank? Bank { get; set; }
        public string? EmployeeType { get; set; } // "تحت التدريب", "تم التعيين"

    }
}
