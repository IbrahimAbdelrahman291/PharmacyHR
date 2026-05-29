using System;
using System.Collections.Generic;
using System.Text;

namespace Requests.Application.DTOs
{
    public class AreaManagerApproveHolidayDto
    {
        public bool IsApproved { get; set; }
        public string? Cover { get; set; } // مين هيغطي
        public string? RejectionReason { get; set; }
    }
}
