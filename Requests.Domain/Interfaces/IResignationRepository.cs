using Requests.Domain.Entities;

namespace Requests.Domain.Interfaces
{
    public interface IResignationRepository
    {
        Task<bool> AddAsync(ResignationRequest request);
        Task<IList<ResignationRequest>> GetAllAsync(int? employeeId, bool? isSeenByHR, int page, int pageSize);
        Task<int> GetTotalCountAsync(int? employeeId, bool? isSeenByHR);
        Task<ResignationRequest?> GetByIdAsync(int id);
        Task<bool> UpdateAsync(ResignationRequest request);
        Task<int> GetUnseenCountAsync(string role);
    }
}