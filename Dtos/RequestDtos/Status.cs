using System.Text.Json.Serialization;

public class StatusDto
{
    //[JsonPropertyName("id")]
    //public string Id { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    //[JsonPropertyName("timestamp")]
    //public string Timestamp { get; set; }

    //[JsonPropertyName("recipient_id")]
    //public string RecipientId { get; set; }
}
