using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

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
                query = query.Where(x => x.Title.Contains(search));

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
        [HttpGet("{uuid}")]
        public async Task<IActionResult> GetById(string uuid)
        {
            var article = await _context.NewsArticles
                .FirstOrDefaultAsync(x => x.Uuid == uuid);

            if (article == null)
                return NotFound();

            using var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Add(
                "x-api-key",
                "c20b9b14837dea7cdd0b5d6c6c0f89704d4515674dcc65dc7b6f85b804cef90b");

            var url =
                $"https://api.freenewsapi.io/v1/details?uuid={uuid}";

            var response =
                await httpClient.GetAsync(url);

            var json =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return Ok(new
                {
                    StatusCode = (int)response.StatusCode,
                    Response = json
                });
            }

            var detail =
                JsonSerializer.Deserialize<FreeNewsDetailResponse>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            return Ok(new
            {
                article.Id,
                article.Uuid,
                article.Title,
                article.ImageUrl,
                article.Source,
                article.PublishedAt,
                Body = detail?.Data?.Body
            });
        }
        [HttpPost("sync")]
        public async Task<IActionResult> Sync([FromServices] NewsSyncJob job)
        {
            await job.ExecuteAsync();
            return Ok();
        }
    }
}