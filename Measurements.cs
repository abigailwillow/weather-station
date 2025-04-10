using Microsoft.ApplicationInsights.Extensibility.Implementation;

namespace WeatherStation;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WeatherStation.Models;
using WeatherStation.Utilities;

public class Measurements {
    const string WEATHER_API_URL = "https://data.buienradar.nl/2.0/feed/json";
    const string IMAGE_API_URL = "http://picsum.photos/1024/1024";
    private readonly ILogger log;

    public Measurements(ILoggerFactory loggerFactory) {
        log = loggerFactory.CreateLogger<Measurements>();
    }

    [Function("Measurements")]
    public async Task<HttpResponseData> GetMeasurements([HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "measurements")] HttpRequestData request) {
        HttpResponseData response = request.CreateResponse();
        response.WriteString(JsonSerializer.Serialize(new {x = Guid.NewGuid().ToString()}));
        return response;
    }

    [Function("GenerateImage")]
    public async Task<string> GenerateImage([QueueTrigger("images")] Guid jobUuid, FunctionContext context) {
        HttpClient client = new();
        HttpResponseMessage httpResponse = await client.GetAsync(WEATHER_API_URL);
        httpResponse.EnsureSuccessStatusCode();
        string json =  await httpResponse.Content.ReadAsStringAsync();

        JsonDocument weatherData = JsonDocument.Parse(json);
        JsonElement measurementsData = weatherData.RootElement.GetProperty("actual").GetProperty("stationmeasurements");
        List<Measurement> measurements = new List<Measurement>();
        foreach (JsonElement measurementData in measurementsData.EnumerateArray()) {
            Measurement measurement = JsonSerializer.Deserialize<Measurement>(measurementData);
            measurements.Add(measurement);
        }

        string firstStationName = measurements[0].StationName;
        string firstTemperature = $"{measurements[0].Temperature}°C";
        
        byte[] image = await GetRandomImage();
        image = ImageUtility.AddTextToImage(image, (firstStationName, (16, 16), 32, "#FFFFFF"));
        image = ImageUtility.AddTextToImage(image, (firstTemperature, (16, 48), 32, "#FFFFFF"));
        
        log.LogInformation(image.ToString());
        return string.Empty;
    }

    private async Task<byte[]> GetRandomImage() {
        HttpClient client = new();
        HttpResponseMessage response = await client.GetAsync(IMAGE_API_URL);
        return await response.Content.ReadAsByteArrayAsync();
    }
}
