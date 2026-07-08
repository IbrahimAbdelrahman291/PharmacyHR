

namespace Requests.Application.DTOs
{
    public class OvertimeRequestDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string NameOfLateEmployee { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public double Hours { get; set; }
        public string? Notes { get; set; }
        public DateTime DateOfShift { get; set; }
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ControlApproval { get; set; }
        public string? ControlRejectionReason { get; set; }
        public string? AreaManagerApproval { get; set; }
        public string? AreaManagerRejectionReason { get; set; }
        public string? HRApproval { get; set; }
        public string? HRRejectionReason { get; set; }
        public bool IsSeenByHR { get; set; }
        public bool IsSeenByControl { get; set; }
        public bool IsSeenByAreaManager { get; set; }
    }
}
