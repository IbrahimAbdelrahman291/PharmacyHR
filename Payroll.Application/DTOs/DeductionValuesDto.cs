using System;
using System.Collections.Generic;
using System.Text;

namespace Payroll.Application.DTOs
{
    public class DeductionValuesDto
    {
        public double HalfDay { get; set; }
        public double OneDay { get; set; }
        public double TwoDays { get; set; }
        public double ThreeDays { get; set; }
        public double FiveDays { get; set; }
        public double TenDays { get; set; }
    }
}
