using SharedKernel.Common;

namespace Attendance.Domain.Entities
{
    public class WorkLog : BaseEntity
    {
        public int EmployeeId { get; set; }
        public DateOnly Day { get; set; }
        public TimeOnly Start { get; set; }
        public TimeOnly End { get; set; }
        public TimeSpan TotalTime { get; set; }
    }
}
