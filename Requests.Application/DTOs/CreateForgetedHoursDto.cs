using System;
using System.Collections.Generic;
using System.Text;

namespace Requests.Application.DTOs
{
    public class CreateForgetedHoursDto
    {
        public string Reason { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateOnly ShiftDate { get; set; }
    }
}
