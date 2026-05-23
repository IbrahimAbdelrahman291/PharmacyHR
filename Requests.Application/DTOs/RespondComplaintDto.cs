using System;
using System.Collections.Generic;
using System.Text;

namespace Requests.Application.DTOs
{
    public class RespondComplaintDto
    {
        public string Response { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // Resolved, Rejected
    }
}
