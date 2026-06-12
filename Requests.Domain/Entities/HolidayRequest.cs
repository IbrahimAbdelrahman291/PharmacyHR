using SharedKernel.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Requests.Domain.Entities
{
    public class HolidayRequest : BaseEntity
    {
        public int EmployeeId { get; set; }
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
        public int TotalDays { get; set; }
        public string Status { get; set; } = "Pending";
        public string? AreaManagerApproval { get; set; } // Approved, Rejected
        public string? AreaManagerCover { get; set; } // مين هيغطي
        public string? AreaManagerUserId { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime RequestDate { get; set; }
        public bool IsSeenByHR { get; set; } = false;
        public bool IsSeenByEmployee { get; set; } = true;
    }
}
