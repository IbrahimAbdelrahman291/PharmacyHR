using SharedKernel.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Requests.Domain.Entities
{
    public class BorrowRequest : BaseEntity
    {
        public int EmployeeId { get; set; }
        public double Amount { get; set; }
        public string? Notes { get; set; }
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } = "Pending";
        public string? RejectionReason { get; set; }
        public bool IsSeenByHR { get; set; } = false;
        public bool IsOverQuarter { get; set; } = false; // تنبيه لو أكتر من ربع المرتب
    }
}
