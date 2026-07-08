using Branches.Application.DTOs;
using SharedKernel.Wrappers;


namespace Branches.Application.Interfaces
{
    public interface IBranchService
    {
        Task<Result<bool>> AddAsync(CreateBranchDto dto);
        Task<Result<bool>> UpdateAsync(int id, UpdateBranchDto dto);
        Task<Result<bool>> DeleteAsync(int id);
        Task<Result<PaginatedResponse<BranchDto>>> GetAllAsync(int page, int pageSize);
    }
}
