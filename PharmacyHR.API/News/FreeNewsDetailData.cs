using System.Text.Json.Serialization;

namespace PharmacyHR.API.News
{
    public class FreeNewsDetailData
    {
        [JsonPropertyName("body")]
        public string Body { get; set; } = string.Empty;
    }
}
