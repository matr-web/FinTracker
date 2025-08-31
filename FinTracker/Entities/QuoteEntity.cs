using System.Text.Json.Serialization;

namespace FinTracker.WebAPI.Entities;

public class QuoteEntity
{
    [JsonPropertyName("c")]
    public decimal CurrentPrice { get; set; }

    [JsonPropertyName("h")]
    public decimal HighOfTheDay { get; set; }

    [JsonPropertyName("l")]
    public decimal LowOfTheDay { get; set; }

    [JsonPropertyName("o")]
    public decimal OpenPrice { get; set; }

    [JsonPropertyName("pc")]
    public decimal PreviousClosePrice { get; set; }

    [JsonPropertyName("t")]
    public long Timestamp { get; set; }
}
