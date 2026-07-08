using Identity.Domain.Entities;
using Identity.Domain.Interfaces;
using Identity.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Repositories
{
    public class AuthRepository : Identity.Domain.Interfaces.IAuthRepository, SharedKernel.Interfaces.IAuthRepository
    {
        private readonly UserManager<User> _userManager;

        private readonly IdentityDbContext _context;

        public AuthRepository(UserManager<User> userManager, IdentityDbContext context)
        {
            _userManager = userManager;
            _context = context;
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

        public async Task<IList<User>> GetAllUsersAsync(int page, int pageSize,string? name)
        {
            if (name is not null)
            {
                return _userManager.Users.Where(x => x.Name.Contains(name))
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new User
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Name = u.Name,
                    EmployeeId = u.EmployeeId,
                    BranchId = u.BranchId,
                    IsActive = u.IsActive
                })
                .ToList();
            }

            return _userManager.Users
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new User
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Name = u.Name,
                    EmployeeId = u.EmployeeId,
                    BranchId = u.BranchId,
                    IsActive = u.IsActive
                })
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
        public async Task<bool> ToggleUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return false;
            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);
            return true;
        }
        public async Task<bool> AddAreaManagerBranchesAsync(string userId, IList<int> branchIds)
        {
            var result = (string?)null;
            foreach (var branch in branchIds)
            {
                result = await GetAreaManagerByBranchIdAsync(branch);
            }
            if (result is null)
            {

                var branches = branchIds.Select(branchId => new AreaManagerBranch
                {
                    UserId = userId,
                    BranchId = branchId
                }).ToList();

                await _context.AreaManagerBranches.AddRangeAsync(branches);
                await _context.SaveChangesAsync();
                return true;
            }
            else
            {
                return false;
            }

        }

        public async Task<IList<int>> GetAreaManagerBranchesAsync(string userId)
            => await _context.AreaManagerBranches
                .Where(x => x.UserId == userId)
                .Select(x => x.BranchId)
                .ToListAsync();
        public async Task<string?> GetAreaManagerByBranchIdAsync(int branchId)
        {
            var areaManagerBranch = await _context.AreaManagerBranches
                .FirstOrDefaultAsync(x => x.BranchId == branchId);

            return areaManagerBranch?.UserId;
        }
        public async Task<bool> AddAreaManagerBranchAsync(string userId, int branchId)
        {
            var result = await GetAreaManagerByBranchIdAsync(branchId);
            
            if (result is not null) return false;
            await _context.AreaManagerBranches.AddAsync(new AreaManagerBranch
            {
                UserId = userId,
                BranchId = branchId
            });
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveAreaManagerBranchAsync(string userId, int branchId)
        {
            var branch = await _context.AreaManagerBranches
                .FirstOrDefaultAsync(x => x.UserId == userId && x.BranchId == branchId);
            if (branch is null) return false;

            _context.AreaManagerBranches.Remove(branch);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> FindByUsername(string username)
        {
            var result = await _userManager.FindByNameAsync(username);
            return result is not null ? true : false;

        }

        public async Task<bool> DeleteUser(int id)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.EmployeeId == id);
            if (user is null) return false;
            var isDeleted = await _userManager.DeleteAsync(user);
            return isDeleted.Succeeded == true ? true : false;
        }
    }
}