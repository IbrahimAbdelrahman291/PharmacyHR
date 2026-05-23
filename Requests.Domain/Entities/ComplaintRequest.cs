using SharedKernel.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Requests.Domain.Entities
{
    public class ComplaintRequest : BaseEntity
    {
        public int EmployeeId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string RecipientRole { get; set; } = string.Empty; // HR, AreaManager, CEO
        public string? RecipientUserId { get; set; } // UserId بتاع الـ AreaManager المسؤل
        public string Status { get; set; } = "Pending";
        public string? Response { get; set; }
        public DateTime Date { get; set; }
        public bool IsSeenByHR { get; set; } = false;
    }
}
