using System.Text.Json.Serialization;

namespace WeatherStation.Models;

public class Measurement {
    [JsonPropertyName("stationid")]
    public int StationId { get; set; }

    [JsonPropertyName("stationname")]
    public string StationName { get; set; }

    [JsonPropertyName("temperature")]
    public float Temperature { get; set; }
}
