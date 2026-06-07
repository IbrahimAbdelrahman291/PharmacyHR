using System.Text.Json;

namespace PharmacyHR.API.News
{
    public class FreeNewsApiProvider : INewsProvider
    {
        private readonly HttpClient _httpClient;

        private const string ApiKey =
            "c20b9b14837dea7cdd0b5d6c6c0f89704d4515674dcc65dc7b6f85b804cef90b";

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
                $"{BaseUrl}?language=ar&country=eg&q={Uri.EscapeDataString(keyword)}";

            var response =
                await _httpClient.GetAsync(url);

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
                    Title = x.Title ?? string.Empty,

                    Description = string.Empty,

                    Url = x.OriginalUrl ?? string.Empty,

                    ImageUrl = x.Thumbnail,

                    Source = x.Publisher ?? string.Empty,

                    PublishedAt = x.PublishedAt
                })
                .ToList();
        }
    }
}