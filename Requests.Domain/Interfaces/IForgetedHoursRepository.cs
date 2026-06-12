using Requests.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Requests.Domain.Interfaces
{
    public interface IForgetedHoursRepository
    {
        Task<bool> AddAsync(ForgetedHoursRequest request);
        Task<IList<ForgetedHoursRequest>> GetAllAsync(int? employeeId, bool? isSeenByHR, int page, int pageSize);
        Task<int> GetTotalCountAsync(int? employeeId, bool? isSeenByHR);
        Task<ForgetedHoursRequest?> GetByIdAsync(int id);
        Task<bool> UpdateAsync(ForgetedHoursRequest request);
        Task<int> GetUnseenCountAsync(string role);
        Task<int> GetMonthlyCountAsync(int employeeId, int month, int year);
    }
}
