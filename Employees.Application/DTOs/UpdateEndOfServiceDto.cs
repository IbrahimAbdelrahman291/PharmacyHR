using System;
using System.Collections.Generic;
using System.Text;

namespace Employees.Application.DTOs
{
    public class UpdateEndOfServiceDto
    {
        public DateTime EndOfServiceDate { get; set; }
        public string EndOfServiceReason { get; set; } = string.Empty;
        public string EndOfServiceType { get; set; } = string.Empty; // استقالة / إنهاء من الشركة / بدون علم الشركة
    }
}
