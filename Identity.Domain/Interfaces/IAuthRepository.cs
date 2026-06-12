using Identity.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Domain.Interfaces
{
    public interface IAuthRepository
    {
        Task<User?> FindByUsernameAsync(string username);
        Task<bool> CheckPasswordAsync(User user, string password);
        Task<IList<string>> GetRolesAsync(User user);
        Task<bool> CreateUserAsync(User user, string password, string role);
        Task<bool> ChangePasswordAsync(User user, string newPassword);
        Task<User?> FindByIdAsync(string userId);
        Task<IList<User>> GetAllUsersAsync(int page, int pageSize);
        Task<int> GetTotalUsersCountAsync();
        Task<bool> ToggleUserAsync(string userId);
        Task<bool> AddAreaManagerBranchesAsync(string userId, IList<int> branchIds);
        Task<IList<int>> GetAreaManagerBranchesAsync(string userId);
        Task<bool> AddAreaManagerBranchAsync(string userId, int branchId);
        Task<bool> RemoveAreaManagerBranchAsync(string userId, int branchId);
    }
}
