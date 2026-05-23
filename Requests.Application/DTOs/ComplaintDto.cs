using System;
using System.Collections.Generic;
using System.Text;

namespace Requests.Application.DTOs
{
    public class ComplaintDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string RecipientRole { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Response { get; set; }
        public DateTime Date { get; set; }
        public bool IsSeenByHR { get; set; }
    }
}
