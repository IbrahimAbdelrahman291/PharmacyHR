namespace PharmacyHR.API.News
{
    public class NewsArticle
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Url { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public string Source { get; set; } = null!;
        public DateTime PublishedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
