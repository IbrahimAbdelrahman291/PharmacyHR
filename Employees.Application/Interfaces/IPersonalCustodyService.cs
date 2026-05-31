using Employees.Application.DTOs;
using SharedKernel.Wrappers;

namespace Employees.Application.Interfaces
{
    public interface IPersonalCustodyService
    {
        Task<Result<bool>> AddAsync(int employeeId, CreatePersonalCustodyDto dto);
        Task<Result<bool>> DeleteAsync(int id);
        Task<Result<IList<PersonalCustodyDto>>> GetByEmployeeIdAsync(int employeeId);
    }
}