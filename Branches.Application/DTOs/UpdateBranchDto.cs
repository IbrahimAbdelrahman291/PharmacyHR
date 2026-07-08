using System;
using System.Collections.Generic;
using System.Text;

namespace Branches.Application.DTOs
{
    public class UpdateBranchDto
    {
        public int? TargetNumberOfEmployees { get; set; } = 0;
        public int? TargetSalaries { get; set; } = 0;
        public double? TargetHours { get; set; } = 0.0;
    }
}
