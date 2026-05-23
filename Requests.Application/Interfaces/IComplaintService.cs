using Requests.Application.DTOs;
using SharedKernel.Wrappers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Requests.Application.Interfaces
{
    public interface IComplaintService
    {
        Task<Result<bool>> AddAsync(int employeeId, CreateComplaintDto dto);
        Task<Result<PaginatedResponse<ComplaintDto>>> GetAllAsync(int? employeeId, bool? isSeenByHR, int page, int pageSize);
        Task<Result<bool>> RespondAsync(int id, RespondComplaintDto dto);
        Task<Result<int>> GetUnseenCountAsync();
        Task<Result<bool>> MarkAsSeenAsync(int id);
    }
}
