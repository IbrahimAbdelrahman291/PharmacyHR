using Employees.Application.DTOs;
using SharedKernel.Wrappers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Employees.Application.Interfaces
{
    public interface IEvaluationService
    {
        Task<Result<bool>> AddEvaluationAsync(CreateEvaluationDto dto, string evaluatedBy);
        Task<Result<IList<EvaluationDto>>> GetEvaluationsAsync(int employeeId);
    }
}
