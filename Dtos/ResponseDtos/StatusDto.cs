using System.Text.Json.Serialization;

namespace CustomerService.API.Dtos.RequestDtos
{
    public class StatusDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;
    }
}