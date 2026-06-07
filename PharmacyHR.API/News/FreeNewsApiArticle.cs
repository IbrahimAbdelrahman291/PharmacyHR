using System.Text.Json.Serialization;

namespace PharmacyHR.API.News
{
    public class FreeNewsApiArticle
    {

        [JsonPropertyName("uuid")]
        public string? Uuid { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("publisher")]
        public string? Publisher { get; set; }

        [JsonPropertyName("thumbnail")]
        public string? Thumbnail { get; set; }

        [JsonPropertyName("original_url")]
        public string? OriginalUrl { get; set; }

        [JsonPropertyName("published_at")]
        public DateTime PublishedAt { get; set; }
    }
}
