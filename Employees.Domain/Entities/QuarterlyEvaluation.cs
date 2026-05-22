using SharedKernel.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Employees.Domain.Entities
{
    public class QuarterlyEvaluation : BaseEntity
    {
        public int EmployeeId { get; set; }
        public string EvaluatedBy { get; set; } = string.Empty;
        public string Quarter { get; set; } = string.Empty; // Q1, Q2, Q3, Q4
        public int Year { get; set; }
        public ICollection<EvaluationResult> EvaluationResults { get; set; } = new List<EvaluationResult>();
    }
}
