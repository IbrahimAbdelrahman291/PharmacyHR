using Microsoft.EntityFrameworkCore;

namespace PharmacyHR.API.News
{
    public class NewsSyncJob
    {
        private readonly INewsProvider _newsProvider;
        private readonly NewsDbContext _context;

        private static readonly string[] Keywords =
        {
            "pharmacy",
            "medicine",
            "drug",
            "pharmaceutical",
            "healthcare",
            "FDA",
            "clinical trial",
            "drug shortage"
        };

        public NewsSyncJob(
            INewsProvider newsProvider,
            NewsDbContext context)
        {
            _newsProvider = newsProvider;
            _context = context;
        }

        public async Task ExecuteAsync()
        {
            var tasks = Keywords
                .Select(keyword => _newsProvider.FetchNewsAsync(keyword));

            var results = await Task.WhenAll(tasks);

            var articles = results
                .SelectMany(x => x)
                .Where(x => !string.IsNullOrWhiteSpace(x.Url))
                .GroupBy(x => x.Url)
                .Select(x => x.First())
                .ToList();

            var existingUrls = await _context.NewsArticles
                .AsNoTracking()
                .Select(x => x.Url)
                .ToHashSetAsync();

            var newArticles = articles
                .Where(x => !existingUrls.Contains(x.Url))
                .Select(article => new NewsArticle
                {
                    Id = Guid.NewGuid(),
                    Title = article.Title,
                    Description = article.Description,
                    Url = article.Url,
                    ImageUrl = article.ImageUrl,
                    Source = article.Source,
                    PublishedAt = article.PublishedAt,
                    CreatedAt = DateTime.UtcNow
                })
                .ToList();

            if (newArticles.Count > 0)
            {
                await _context.NewsArticles.AddRangeAsync(newArticles);
                await _context.SaveChangesAsync();
            }
        }
    }
}