using Employees.Application.DTOs;
using SharedKernel.Wrappers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Employees.Application.Interfaces
{
    public interface IEvaluationCriteriaService
    {
        Task<Result<bool>> AddAsync(CreateEvaluationCriteriaDto dto);
        Task<Result<PaginatedResponse<EvaluationCriteriaDto>>> GetAllAsync(int page, int pageSize);
        Task<Result<bool>> DeleteAsync(int id);
    }
}
