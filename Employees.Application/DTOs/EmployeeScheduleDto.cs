using System;
using System.Collections.Generic;
using System.Text;

namespace Employees.Application.DTOs
{
    public class EmployeeScheduleDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly CheckInTime { get; set; }
        public TimeOnly CheckOutTime { get; set; }
    }

}
