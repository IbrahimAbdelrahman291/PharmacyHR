using System;
using System.Collections.Generic;
using System.Text;

namespace Payroll.Application.DTOs
{
    public class UpdateMonthlyDataDto
    {
        public double? TotalSalary { get; set; }      // Static بس
        public double? SalaryPerHour { get; set; }    // Changable/Delivery بس
        public double? Insurence { get; set; }
        public double? HoursOverTime { get; set; }
        public double? ForgetedHours { get; set; }

    }
}
