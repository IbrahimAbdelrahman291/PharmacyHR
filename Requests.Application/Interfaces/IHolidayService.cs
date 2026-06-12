using Requests.Application.DTOs;
using SharedKernel.Wrappers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Requests.Application.Interfaces
{
    public interface IHolidayService
    {
        Task<Result<bool>> AddAsync(int employeeId, CreateHolidayDto dto);
        Task<Result<PaginatedResponse<HolidayDto>>> GetAllAsync(int? employeeId, bool? isSeenByHR, string? areaManagerUserId, string role, int page, int pageSize);
        Task<Result<bool>> AreaManagerApproveAsync(int id, string areaManagerUserId, AreaManagerApproveHolidayDto dto);
        Task<Result<bool>> HRApproveAsync(int id, HRApproveHolidayDto dto);
        Task<Result<int>> GetUnseenCountAsync(string role);
        Task<Result<bool>> MarkAsSeenAsync(int id, string role);
    }
}
