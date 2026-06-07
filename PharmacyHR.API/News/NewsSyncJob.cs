using Microsoft.EntityFrameworkCore;

namespace PharmacyHR.API.News
{
    public class NewsSyncJob
    {
        private readonly INewsProvider _newsProvider;
        private readonly NewsDbContext _context;

        private static readonly string[] Keywords =
        {
            "medicine"
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

            foreach (var keyword in Keywords)
            {
                var result = await _newsProvider.FetchNewsAsync(keyword);
                articles.AddRange(result);

                await Task.Delay(2000);
            }

            articles = articles
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

                    Uuid = article.Uuid,

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