namespace WeatherStation.Models;

using System.Text.Json.Serialization;

public class JobsHttpResponse
{
    [JsonPropertyName("status")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public JobStatus Status { get; set; }
    
    [JsonPropertyName("images")]
    public List<string>? Images { get; set; }
}