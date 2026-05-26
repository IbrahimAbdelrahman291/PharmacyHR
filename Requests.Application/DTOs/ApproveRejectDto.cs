using System;
using System.Collections.Generic;
using System.Text;

namespace Requests.Application.DTOs
{
    public class ApproveRejectDto
    {
        public bool IsApproved { get; set; }
        public string? RejectionReason { get; set; }
    }
}
