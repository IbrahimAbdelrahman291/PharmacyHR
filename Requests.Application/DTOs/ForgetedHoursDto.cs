using System;
using System.Collections.Generic;
using System.Text;

namespace Requests.Application.DTOs
{
    public class ForgetedHoursDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateOnly ShiftDate { get; set; }
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
        public bool IsSeenByHR { get; set; }
    }
}
