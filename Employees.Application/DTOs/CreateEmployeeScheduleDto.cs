using System;
using System.Collections.Generic;
using System.Text;

namespace Employees.Application.DTOs
{
    public class CreateEmployeeScheduleDto
    {
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly CheckInTime { get; set; }
        public TimeOnly CheckOutTime { get; set; }
    }
}
