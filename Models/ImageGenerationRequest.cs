namespace WeatherStation.Models;

public struct ImageGenerationRequest {
    public string JobId { get; init; }
    public int MeasurementIndex { get; init; }
    public string StationName { get; init; }
    public float Temperature { get; init; }
}
