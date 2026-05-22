using System;
using System.Collections.Generic;
using System.Text;

namespace Employees.Application.DTOs
{
    public class EvaluationResultDto
    {
        public int EvaluationCriteriaId { get; set; }
        public string CriteriaName { get; set; } = string.Empty;
        public string Rating { get; set; } = string.Empty;
    }
}
