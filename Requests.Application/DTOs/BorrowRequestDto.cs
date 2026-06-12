using System;
using System.Collections.Generic;
using System.Text;

namespace Requests.Application.DTOs
{
    public class BorrowRequestDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public double Amount { get; set; }
        public string? Notes { get; set; }
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
        public bool IsSeenByHR { get; set; }
        public bool IsSeenByEmployee { get; set; }
        public bool IsOverQuarter { get; set; }
    }
}
