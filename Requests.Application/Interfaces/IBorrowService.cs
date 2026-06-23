using Requests.Application.DTOs;
using SharedKernel.Wrappers;

namespace Requests.Application.Interfaces
{
    public interface IBorrowService
    {
        Task<Result<bool>> AddBorrowRequestAsync(int employeeId, CreateBorrowRequestDto dto);
        Task<Result<PaginatedResponse<BorrowRequestDto>>> GetAllBorrowRequestsAsync(int? employeeId, bool? isSeenByHR, int page, int pageSize);
        Task<Result<bool>> ApproveBorrowRequestAsync(int id, ApproveRejectDto dto);
        Task<Result<int>> GetUnseenBorrowCountAsync(string role, int? employeeId);
        Task<Result<bool>> MarkBorrowAsSeenAsync(int id, string role);
        Task<Result<bool>> AddInstallmentBorrowAsync(CreateInstallmentBorrowDto dto);
        Task<Result<IList<InstallmentBorrowDto>>> GetInstallmentBorrowsByEmployeeAsync(int employeeId);
        Task ProcessMonthlyInstallmentsAsync();
    }
}
