using Requests.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Requests.Domain.Interfaces
{
    public interface IHolidayRepository
    {
        Task<bool> AddAsync(HolidayRequest request);
        Task<IList<HolidayRequest>> GetAllAsync(int? employeeId, bool? isSeenByHR, string? areaManagerUserId, int page, int pageSize);
        Task<int> GetTotalCountAsync(int? employeeId, bool? isSeenByHR, string? areaManagerUserId);
        Task<HolidayRequest?> GetByIdAsync(int id);
        Task<bool> UpdateAsync(HolidayRequest request);
        Task<int> GetUnseenCountAsync(string role, int? employeeId);
    }
}
