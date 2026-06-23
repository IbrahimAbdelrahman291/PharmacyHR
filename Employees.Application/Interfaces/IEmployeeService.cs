using Employees.Application.DTOs;
using SharedKernel.Wrappers;


namespace Employees.Application.Interfaces
{
    public interface IEmployeeService
    {
        Task<Result<bool>> CreateAsync(CreateEmployeeDto dto);
        Task<Result<EmployeeDto>> GetByIdAsync(int id);
        Task<Result<PaginatedResponse<EmployeeDto>>> GetAllAsync(int page, int pageSize, int? branchId, int? bankId, string? role, string? name); 
        Task<Result<bool>> UpdateAsync(int id, UpdateEmployeeDto dto, string userId, string userName);
        Task<Result<bool>> DeleteAsync(int id);
        Task<Result<EmployeeHistoryDto>> GetHistoryAsync(int employeeId);
        Task<Result<bool>> UpdateEndOfServiceAsync(int employeeId, UpdateEndOfServiceDto dto);
        Task<Result<IList<EmployeeBranchDto>>> GetEmployeeBranchesAsync(int employeeId);
        Task<Result<bool>> ImportEmployeesData();
    }
}
