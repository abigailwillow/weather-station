namespace WeatherStation.Models;

using System.Text.Json.Serialization;

public struct ImageGenerationHttpResponse {
    [JsonPropertyName("jobId")]
    public string JobId { get; set; }
    [JsonPropertyName("jobUrl")]
    public string JobUrl { get; set; }
}
