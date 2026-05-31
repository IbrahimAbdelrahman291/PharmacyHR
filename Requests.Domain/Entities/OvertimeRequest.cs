using SharedKernel.Common;

namespace Requests.Domain.Entities
{
    public class OvertimeRequest : BaseEntity
    {
        public int EmployeeId { get; set; }
        public double Hours { get; set; }
        public string? Notes { get; set; }
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } = "Pending";

        // Control
        public string? ControlApproval { get; set; } // Approved, Rejected
        public string? ControlUserId { get; set; }
        public string? ControlRejectionReason { get; set; }

        // Area Manager
        public string? AreaManagerApproval { get; set; } // Approved, Rejected
        public string? AreaManagerUserId { get; set; }
        public string? AreaManagerRejectionReason { get; set; }

        // HR
        public string? HRApproval { get; set; }
        public string? HRRejectionReason { get; set; }

        public bool IsSeenByHR { get; set; } = false;
        public bool IsSeenByControl { get; set; } = false;
        public bool IsSeenByAreaManager { get; set; } = false;
    }
}