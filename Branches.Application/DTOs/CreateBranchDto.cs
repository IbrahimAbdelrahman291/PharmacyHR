using System;
using System.Collections.Generic;
using System.Text;

namespace Branches.Application.DTOs
{
    public class CreateBranchDto
    {
        public string Name { get; set; } = string.Empty;
        public int? TargetNumberOfEmployees { get; set; }
        public int? TargetSalaries { get; set; }
        public double? TargetHours { get; set; }
    }
}
