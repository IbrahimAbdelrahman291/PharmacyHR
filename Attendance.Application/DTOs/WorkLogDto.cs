using System;
using System.Collections.Generic;
using System.Text;

namespace Attendance.Application.DTOs
{
    public class WorkLogDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public DateOnly Day { get; set; }
        public TimeOnly Start { get; set; }
        public TimeOnly End { get; set; }
        public double TotalHours { get; set; }
    }
}
