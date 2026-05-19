using System;
using System.Collections.Generic;
using System.Text;

namespace Attendance.Application.DTOs
{
    public class AttendanceReportDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public DateOnly Day { get; set; }
        public TimeOnly ScheduledCheckIn { get; set; }
        public TimeOnly? ActualCheckIn { get; set; }
        public TimeOnly ScheduledCheckOut { get; set; }
        public TimeOnly? ActualCheckOut { get; set; }
    }
}
