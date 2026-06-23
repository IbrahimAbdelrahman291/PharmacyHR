using Branches.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace Branches.Infrastructure.Data
{
    public class BranchesDbContext : DbContext
    {
        public BranchesDbContext(DbContextOptions<BranchesDbContext> options) : base(options) { }

        public DbSet<Branch> Branches { get; set; }
    }
}
