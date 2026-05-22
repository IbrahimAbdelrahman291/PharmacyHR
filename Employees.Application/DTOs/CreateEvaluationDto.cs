using System;
using System.Collections.Generic;
using System.Text;

namespace Employees.Application.DTOs
{
    public class CreateEvaluationDto
    {
        public int EmployeeId { get; set; }
        public string Quarter { get; set; } = string.Empty;
        public int Year { get; set; }
        public IList<EvaluationResultItemDto> Results { get; set; } = new List<EvaluationResultItemDto>();
    }
}
