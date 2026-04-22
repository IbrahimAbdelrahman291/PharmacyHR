using Branches.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Branches.Domain.Interfaces
{
    public interface IBranchRepository
    {
        Task<bool> AddAsync(Branch branch);
        Task<bool> DeleteAsync(int id);
        Task<IList<Branch>> GetAllAsync(int page, int pageSize);
        Task<int> GetTotalCountAsync();
        Task<Branch?> GetByIdAsync(int id);
    }
}
