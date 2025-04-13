using Azure.Storage.Queues;

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
    private readonly ILogger _log;
    private readonly QueueClient _queue;

    public Measurements(ILoggerFactory loggerFactory, QueueServiceClient queueServiceClient) {
        _log = loggerFactory.CreateLogger<Measurements>();
        _queue = queueServiceClient.GetQueueClient("image-generation-queue");
        _queue.CreateIfNotExists();
    }

    [Function(nameof(GetMeasurements))]
    public async Task<HttpResponseData> GetMeasurements([HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "measurements")] HttpRequestData request) {
        string jobId = Guid.NewGuid().ToString();
        string jobUrl = $"{request.Url.GetLeftPart(UriPartial.Authority)}/api/jobs/{jobId}";

        _queue.SendMessageAsync(JsonSerializer.Serialize(new {jobId = jobId}));
        
        HttpResponseData response = request.CreateResponse();
        response.WriteString(JsonSerializer.Serialize(new {job = jobUrl}));
        return response;
    }

    [Function(nameof(GenerateImage))]
    public async Task<string> GenerateImage([QueueTrigger("image-generation-queue")] string jobId, FunctionContext context) {
        _log.LogInformation($"Started job {jobId}"); 
        
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
        
        _log.LogInformation(image.ToString());
        return string.Empty;
    }

    private static async Task<byte[]> GetRandomImage() {
        HttpClient client = new();
        HttpResponseMessage response = await client.GetAsync(IMAGE_API_URL);
        return await response.Content.ReadAsByteArrayAsync();
    }
}
