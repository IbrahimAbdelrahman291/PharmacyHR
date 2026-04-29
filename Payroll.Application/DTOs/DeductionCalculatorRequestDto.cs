using System;
using System.Collections.Generic;
using System.Text;

namespace Payroll.Application.DTOs
{
    public class DeductionCalculatorRequestDto
    {
        public IList<int> EmployeeIds { get; set; } = new List<int>();
    }
}
