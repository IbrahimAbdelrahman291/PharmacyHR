using System;
using System.Collections.Generic;
using System.Text;

namespace Employees.Application.DTOs
{
    public class EvaluationDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EvaluatedBy { get; set; } = string.Empty;
        public string Quarter { get; set; } = string.Empty;
        public int Year { get; set; }
        public IList<EvaluationResultDto> Results { get; set; } = new List<EvaluationResultDto>();
    }
}
