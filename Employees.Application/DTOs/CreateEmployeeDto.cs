using System;
using System.Collections.Generic;
using System.Text;

namespace Employees.Application.DTOs
{
    public class CreateEmployeeDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? theNameOfJob { get; set; }
        public double? TotalSalary { get; set; }    // للـ Static بس
        public double? SalaryPerHour { get; set; }  // للـ Changable/Delivery بس
        public int? BankId { get; set; }
        public string? BankAccount { get; set; }
        public double? ShiftHours { get; set; }
        public int BranchId { get; set; }
        public DateTime HiringDate { get; set; }
        public string Qualification { get; set; } = string.Empty;
        public int GraduationYear { get; set; }
        public string? NationalId { get; set; }
        public string? PhoneNumber { get; set; }
        public double? Insurence { get; set; }
        public int Holidaies { get; set; }
    }
}
