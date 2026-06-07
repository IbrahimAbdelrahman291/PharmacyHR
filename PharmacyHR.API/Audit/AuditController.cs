using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Interfaces;

namespace PharmacyHR.API.Audit
{
    [ApiController]
    [Route("api/audit")]
    [Authorize(Roles = "Admin")]
    public class AuditController : ControllerBase
    {
        private readonly PharmacyHR.API.Audit.IAuditService _aduitService;
        private readonly AuditDbContext _context;

        public AuditController(PharmacyHR.API.Audit.IAuditService aduitService, AuditDbContext context)
        {
            _aduitService = aduitService;
            _context = context;
        }

        public async Task<IActionResult> GetAll(
            [FromQuery] DateTime? createdAt = null,
            [FromQuery] string? userId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(x => x.UserId == userId);

            if (createdAt.HasValue)
                query = query.Where(x => x.CreatedAt >= createdAt.Value);

            var totalCount = await query.CountAsync();

            var logs = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new { data = logs, totalCount, page, pageSize });
        }
    }
}
