using SharedKernel.Common;

namespace Requests.Domain.Entities
{
    public class ResignationRequest : BaseEntity
    {
        public int EmployeeId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } = "Pending";
        public string? RejectionReason { get; set; }
        public bool IsSeenByHR { get; set; } = false;
        public bool IsSeenByEmployee { get; set; } = true;
    }
}