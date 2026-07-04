using Microsoft.EntityFrameworkCore;

namespace PharmacyHR.API.News
{
    public class NewsSyncJob
    {
        private readonly INewsProvider _newsProvider;
        private readonly NewsDbContext _context;

        private static readonly string[] Keywords =
        {
            "medicine",
            "science"
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
            var articles = new List<NewsArticleResult>();

            var result = await _newsProvider.FetchNewsAsync("medicine");
            articles.AddRange(result);

            var existingUuids = await _context.NewsArticles
                .AsNoTracking()
                .Select(x => x.Uuid)
                .ToHashSetAsync();

            var newArticles = articles
                .Where(x => !existingUuids.Contains(x.Uuid))
                .Select(article => new NewsArticle
                {
                    Id = Guid.NewGuid(),

                    Uuid = article.Uuid,

                    Title = article.Title,

                    ImageUrl = article.ImageUrl,

                    Source = article.Source,

                    PublishedAt = article.PublishedAt,

                    CreatedAt = DateTime.UtcNow
                })
                .ToList();

            if (newArticles.Any())
            {
                await _context.NewsArticles.AddRangeAsync(newArticles);
                await _context.SaveChangesAsync();
            }
        }
    }
}