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
        Task<Result<PaginatedResponse<ComplaintDto>>> GetAllAsync(int? employeeId, bool? isSeenByHR, string? recipientUserId, string role, int page, int pageSize);
        Task<Result<bool>> RespondAsync(int id, RespondComplaintDto dto, string userId, string role);
        Task<Result<int>> GetUnseenCountAsync(string? recipientUserId, string role, int? employeeId);
        Task<Result<bool>> MarkAsSeenAsync(int id, string userId, string role);
    }
}
