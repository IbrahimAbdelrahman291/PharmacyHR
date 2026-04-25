using System;
using System.Collections.Generic;
using System.Text;

namespace Employees.Application.DTOs
{
    public class EmployeeHistoryDto
    {
        public int EmployeeId { get; set; }
        public DateTime HiringDate { get; set; }
        public string Qualification { get; set; } = string.Empty;
        public int GraduationYear { get; set; }
        public string? NationalId { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? EndOfServiceDate { get; set; }
        public string? EndOfServiceReason { get; set; }
        public string? EndOfServiceType { get; set; }
    }
}
