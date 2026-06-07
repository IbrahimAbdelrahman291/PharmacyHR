using System.Text.Json.Serialization;

namespace PharmacyHR.API.News
{
    public class FreeNewsDetailResponse
    {
        [JsonPropertyName("data")]
        public FreeNewsDetailData Data { get; set; } = null!;
    }
}
