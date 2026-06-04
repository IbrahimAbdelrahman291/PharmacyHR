using Microsoft.EntityFrameworkCore;

namespace PharmacyHR.API.News
{
    public class NewsDbContext : DbContext
    {
        public NewsDbContext(DbContextOptions<NewsDbContext> options) : base(options) { }
        public DbSet<NewsArticle> NewsArticles { get; set; }
    }
}
