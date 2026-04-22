using Branches.Domain.Entities;
using Branches.Domain.Interfaces;
using Branches.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Branches.Infrastructure.Repositories
{
    public class BranchRepository : Branches.Domain.Interfaces.IBranchRepository, SharedKernel.Interfaces.IBranchRepository

    {
        private readonly BranchesDbContext _context;

        public BranchRepository(BranchesDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddAsync(Branch branch)
        {
            await _context.Branches.AddAsync(branch);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch is null) return false;

            _context.Branches.Remove(branch);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IList<Branch>> GetAllAsync(int page, int pageSize)
            => await _context.Branches
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

        public async Task<int> GetTotalCountAsync()
            => await _context.Branches.CountAsync();

        public async Task<Branch?> GetByIdAsync(int id)
            => await _context.Branches.FindAsync(id);

        public async Task<(int Id, string Name)?> GetBranchByIdAsync(int id)
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch is null) return null;
            return (branch.Id, branch.Name);
        }
    }
}
