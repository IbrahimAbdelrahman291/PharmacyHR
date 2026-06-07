using Microsoft.EntityFrameworkCore;

namespace PharmacyHR.API.News
{
    public class NewsDbContext : DbContext
    {
        public NewsDbContext(DbContextOptions<NewsDbContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NewsArticle>()
                .HasIndex(x => x.Uuid)
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
        public DbSet<NewsArticle> NewsArticles { get; set; }
    }
}
