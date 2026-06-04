using Microsoft.EntityFrameworkCore;

namespace PharmacyHR.API.News
{
    public class NewsSyncJob
    {
        private readonly INewsProvider _newsProvider;
        private readonly NewsDbContext _context;

        private static readonly string[] Keywords = new[]
        {
            "pharmacy", "medicine", "drug", "pharmaceutical",
            "healthcare", "FDA", "clinical trial", "drug shortage"
        };

        public NewsSyncJob(INewsProvider newsProvider, NewsDbContext context)
        {
            _newsProvider = newsProvider;
            _context = context;
        }

        public async Task ExecuteAsync()
        {
            foreach (var keyword in Keywords)
            {
                var articles = await _newsProvider.FetchNewsAsync(keyword);

                foreach (var article in articles)
                {
                    // منع التكرار بالـ URL
                    var exists = await _context.NewsArticles
                        .AnyAsync(x => x.Url == article.Url);

                    if (exists) continue;

                    await _context.NewsArticles.AddAsync(new NewsArticle
                    {
                        Id = Guid.NewGuid(),
                        Title = article.Title,
                        Description = article.Description,
                        Url = article.Url,
                        ImageUrl = article.ImageUrl,
                        Source = article.Source,
                        PublishedAt = article.PublishedAt,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                await _context.SaveChangesAsync();
            }
        }
    }
}