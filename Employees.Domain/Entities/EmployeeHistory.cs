using SharedKernel.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Employees.Domain.Entities
{
    public class EmployeeHistory : BaseEntity
    {
        public int EmployeeId { get; set; }
        public DateTime HiringDate { get; set; }
        public string Qualification { get; set; } = string.Empty;
        public int GraduationYear { get; set; }
        public string? NationalId { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? EndOfServiceDate { get; set; }
        public string? EndOfServiceReason { get; set; }
        public string? EndOfServiceType { get; set; } // استقالة / إنهاء من الشركة / بدون علم الشركة
    }
}
