using SharedKernel.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Requests.Domain.Entities
{
    public class ForgetedHoursRequest : BaseEntity
    {
        public int EmployeeId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateOnly ShiftDate { get; set; }
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } = "Pending";
        public string? RejectionReason { get; set; }
        public bool IsSeenByHR { get; set; } = false;
        public bool IsSeenByEmployee { get; set; } = true;
    }
}
