namespace PharmacyHR.API.News
{
    public class FreeNewsApiArticle
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? OriginalUrl { get; set; }
        public string? Thumbnail { get; set; }
        public string? Publisher { get; set; }
        public DateTime PublishedAt { get; set; }
    }
}
