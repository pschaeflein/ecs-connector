using System.Text.Json.Serialization;

namespace ChuckNorrisConnector.Models
{
  public class Joke
  {
    [JsonPropertyName("created_at")]
    public string? Created { get; set; }

    [JsonPropertyName("icon_url")]
    public Uri? IconUrl { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("updated_at")]
    public string? UpdatedAt { get; set; }

    [JsonPropertyName("url")]
    public Uri? Url { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }

    [JsonPropertyName("moderated")]
    public bool Moderated { get; set; }
  }
}
