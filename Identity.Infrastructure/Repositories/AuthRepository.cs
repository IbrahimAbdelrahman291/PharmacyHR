using Microsoft.AspNetCore.Identity;
using Identity.Domain.Entities;
using Identity.Domain.Interfaces;

namespace Identity.Infrastructure.Repositories
{
    public class AuthRepository : Identity.Domain.Interfaces.IAuthRepository, SharedKernel.Interfaces.IAuthRepository
    {
        private readonly UserManager<User> _userManager;

        public AuthRepository(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<User?> FindByUsernameAsync(string username)
            => await _userManager.FindByNameAsync(username);

        public async Task<bool> CheckPasswordAsync(User user, string password)
            => await _userManager.CheckPasswordAsync(user, password);

        public async Task<IList<string>> GetRolesAsync(User user)
            => await _userManager.GetRolesAsync(user);

        public async Task<bool> CreateUserAsync(User user, string password, string role)
        {
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded) return false;

            await _userManager.AddToRoleAsync(user, role);
            return true;
        }

        public async Task<bool> ChangePasswordAsync(User user, string newPassword)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            return result.Succeeded;
        }
        public async Task<User?> FindByIdAsync(string userId)
            => await _userManager.FindByIdAsync(userId);

        public async Task<IList<User>> GetAllUsersAsync(int page, int pageSize)
        {
            return _userManager.Users
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public async Task<int> GetTotalUsersCountAsync()
            => await Task.FromResult(_userManager.Users.Count());

        public async Task<bool> CreateUserAsync(string username, string password, string role, string name, int? employeeId, int? branchId)
        {
            var user = new User
            {
                UserName = username,
                Name = name,
                EmployeeId = employeeId,
                BranchId = branchId
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded) return false;

            await _userManager.AddToRoleAsync(user, role);
            return true;
        }
    }
}