using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PharmacyHR.API.News
{
    [ApiController]
    [Route("api/news")]
    [Authorize]
    public class NewsController : ControllerBase
    {
        private readonly NewsDbContext _context;

        public NewsController(NewsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null)
        {
            var query = _context.NewsArticles.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(x => x.Title.Contains(search) || x.Description.Contains(search));

            var totalCount = await query.CountAsync();

            var articles = await query
                .OrderByDescending(x => x.PublishedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                data = articles,
                totalCount,
                page,
                pageSize
            });
        }
    }
}