using System;
using System.Collections.Generic;
using System.Text;

namespace Requests.Application.DTOs
{
    public class HolidayDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
        public int TotalDays { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? AreaManagerApproval { get; set; }
        public string? AreaManagerCover { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime RequestDate { get; set; }
        public bool IsSeenByHR { get; set; }
    }
}
