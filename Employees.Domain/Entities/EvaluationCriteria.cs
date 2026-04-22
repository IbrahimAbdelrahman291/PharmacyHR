using SharedKernel.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Employees.Domain.Entities
{
    public class EvaluationCriteria : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
    }
}
