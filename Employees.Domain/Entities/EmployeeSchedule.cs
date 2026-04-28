using SharedKernel.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Employees.Domain.Entities
{
    public class EmployeeSchedule : BaseEntity
    {
        public int EmployeeId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly CheckInTime { get; set; }
        public TimeOnly CheckOutTime { get; set; }
    }
}
