namespace PharmacyHR.API.News
{
    public class NewsArticleResult
    {
        public string Uuid { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public string Source { get; set; } = string.Empty;

        public DateTime PublishedAt { get; set; }
    }
}
