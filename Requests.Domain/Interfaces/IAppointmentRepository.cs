using Requests.Domain.Entities;

namespace Requests.Domain.Interfaces
{
    public interface IAppointmentRepository
    {
        Task<bool> AddAsync(AppointmentRequest request);
        Task<IList<AppointmentRequest>> GetAllAsync(bool? isSeenByHR, int page, int pageSize);
        Task<int> GetTotalCountAsync(bool? isSeenByHR);
        Task<AppointmentRequest?> GetByIdAsync(int id);
        Task<bool> UpdateAsync(AppointmentRequest request);
        Task<int> GetUnseenCountAsync();
    }
}