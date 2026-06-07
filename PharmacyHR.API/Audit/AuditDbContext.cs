using Microsoft.EntityFrameworkCore;

namespace PharmacyHR.API.Audit
{
    public class AuditDbContext : DbContext
    {
        public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options) { }
        public DbSet<AuditLog> AuditLogs { get; set; }
    }
}