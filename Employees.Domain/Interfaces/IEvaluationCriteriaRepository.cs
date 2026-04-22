using Employees.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Employees.Domain.Interfaces
{
    public interface IEvaluationCriteriaRepository
    {
        Task AddAsync(EvaluationCriteria criteria);
        Task<IList<EvaluationCriteria>> GetAllAsync(int page, int pageSize);
        Task<int> GetTotalCountAsync();
        Task<bool> DeleteAsync(int id);
    }
}
