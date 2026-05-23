using Requests.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Requests.Domain.Interfaces
{
    public interface IComplaintRepository
    {
        Task<bool> AddAsync(ComplaintRequest complaint);
        Task<IList<ComplaintRequest>> GetAllAsync(int? employeeId, bool? isSeenByHR, int page, int pageSize);
        Task<int> GetTotalCountAsync(int? employeeId, bool? isSeenByHR);
        Task<ComplaintRequest?> GetByIdAsync(int id);
        Task<bool> UpdateAsync(ComplaintRequest complaint);
        Task<int> GetUnseenCountAsync();
    }
}
