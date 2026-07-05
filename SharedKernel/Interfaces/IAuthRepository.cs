

namespace SharedKernel.Interfaces
{
    public interface IAuthRepository
    {
        Task<bool> CreateUserAsync(string username, string password, string role, string name, int? employeeId, int? branchId);
        Task<string?> GetAreaManagerByBranchIdAsync(int branchId);
        Task<bool> FindByUsername(string username);
        Task<bool> DeleteUser(int id);
    }
}
