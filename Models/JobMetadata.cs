namespace WeatherStation.Models;

using System.Text.Json.Serialization;

public struct JobMetadata
{
    [JsonPropertyName("stationCount")]
    public int StationCount { get; init; }
}