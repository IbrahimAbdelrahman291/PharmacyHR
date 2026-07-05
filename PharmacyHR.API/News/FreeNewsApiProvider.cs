using System.Text.Json;

namespace PharmacyHR.API.News
{
    public class FreeNewsApiProvider : INewsProvider
    {
        private readonly HttpClient _httpClient;

        private const string ApiKey =
            "1f8af3768ee37349d0ab41c413e0f8e2187d13746ea8b7c8224d85c38a5cb480";

        private const string BaseUrl =
            "https://api.freenewsapi.io/v1/news";

        public FreeNewsApiProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;

            if (!_httpClient.DefaultRequestHeaders.Contains("x-api-key"))
            {
                _httpClient.DefaultRequestHeaders.Add(
                    "x-api-key",
                    ApiKey);
            }
        }

        public async Task<IList<NewsArticleResult>> FetchNewsAsync(
            string keyword)
        {
            var url =
                $"{BaseUrl}?language=en&in_title={Uri.EscapeDataString(keyword)}";

            var response =
                await _httpClient.GetAsync(url);
            var body = response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                return new List<NewsArticleResult>();
            }

            var json =
                await response.Content.ReadAsStringAsync();

            var result =
                JsonSerializer.Deserialize<FreeNewsApiResponse>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (result?.Data == null)
                return new List<NewsArticleResult>();

            return result.Data
                .Select(x => new NewsArticleResult
                {
                    Uuid = x.Uuid!,

                    Title = x.Title ?? string.Empty,

                    ImageUrl = x.Thumbnail,

                    Source = x.Publisher ?? string.Empty,

                    PublishedAt = x.PublishedAt
                })
                .ToList();
        }
    }
}