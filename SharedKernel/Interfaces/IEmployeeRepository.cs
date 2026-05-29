

namespace SharedKernel.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<(int Id, string Name, int BranchId, string? BankName, string? BankAccount)?> GetEmployeeBasicInfoAsync(int employeeId);
        Task<double?> GetShiftHoursAsync(int employeeId);

    }
}
