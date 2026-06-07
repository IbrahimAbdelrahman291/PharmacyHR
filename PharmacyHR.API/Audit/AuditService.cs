using System.Runtime.CompilerServices;

namespace PharmacyHR.API.Audit
{
    public class AuditService : IAuditService, SharedKernel.Interfaces.IAduitService
    {
        private readonly AuditDbContext _context;

        public AuditService(AuditDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(string userId, string userName, string action)
        {
            await _context.AuditLogs.AddAsync(new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                UserName = userName,
                Action = action,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        public async Task LogDetailsAsync(string userId, string userName, string action)
        {
            await _context.AuditLogs.AddAsync(new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                UserName = userName,
                Action = action,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }
    }
}