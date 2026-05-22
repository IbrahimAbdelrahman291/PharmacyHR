using System;
using System.Collections.Generic;
using System.Text;

namespace Employees.Application.DTOs
{
    public class EvaluationResultItemDto
    {
        public int EvaluationCriteriaId { get; set; }
        public string Rating { get; set; } = string.Empty;
    }
}
