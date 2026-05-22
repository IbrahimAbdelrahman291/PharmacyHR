using SharedKernel.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Employees.Domain.Entities
{
    public class EvaluationResult : BaseEntity
    {
        public int QuarterlyEvaluationId { get; set; }
        public int EvaluationCriteriaId { get; set; }
        public string Rating { get; set; } = string.Empty; // ممتاز, جيد, ضعيف
    }
}
