namespace WeatherStation.Functions;

using System.Text;
using System.Text.Json;

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Queues;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

using WeatherStation.Models;
using WeatherStation.Extensions;
using WeatherStation.Services;

public class RequestImages(ILoggerFactory loggerFactory, IQueueProviderService queueProviderService, IBlobProviderService blobProviderService) {
    private const string WEATHER_API_URL = "https://data.buienradar.nl/2.0/feed/json";
    
    private readonly ILogger _logger = loggerFactory.CreateLogger<RequestImages>();
    private readonly QueueClient _queue = queueProviderService.GetQueueClient();
    private readonly BlobContainerClient _blob = blobProviderService.GetBlobContainerClient();

    [Function(nameof(RequestImages))]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "request-images")] HttpRequestData request) {
        string jobId = Guid.NewGuid().ToString();
        string jobUrl = $"{request.Url.GetLeftPart(UriPartial.Authority)}/api/jobs/{jobId}";

        HttpClient client = new();
        HttpResponseMessage httpResponse = await client.GetAsync(WEATHER_API_URL);
        httpResponse.EnsureSuccessStatusCode();
        string jsonData = await httpResponse.Content.ReadAsStringAsync();
        JsonDocument weatherData = JsonDocument.Parse(jsonData);
        JsonElement measurementsData = weatherData.RootElement.GetProperty("actual").GetProperty("stationmeasurements");
        
        BlobClient metaBlobClient = _blob.GetBlobClient($"{jobId}.json");
        string metadata = JsonSerializer.Serialize(new JobMetadata { StationCount = measurementsData.GetArrayLength() });
        
        using MemoryStream memoryStream = new (Encoding.UTF8.GetBytes(metadata));
        await metaBlobClient.UploadAsync(
            memoryStream,
            new BlobHttpHeaders {
                ContentType = "application/json"
            }
        );
        
        int measurementIndex = 0;
        foreach (JsonElement measurementData in measurementsData.EnumerateArray()) {
            Measurement? measurement = measurementData.Deserialize<Measurement>();
            _queue.SendMessageString(JsonSerializer.Serialize(new ImageGenerationRequest {
                JobId = jobId,
                MeasurementIndex = measurementIndex,
                StationName = measurement?.StationName ?? string.Empty,
                Temperature = measurement?.Temperature ?? 0.0f
            }));
            
            measurementIndex += 1;
        }

        HttpResponseData response = request.CreateResponse();
        await response.WriteStringAsync(JsonSerializer.Serialize(new ImageGenerationHttpResponse {
            JobId = jobId,
            JobUrl = jobUrl
        }));
        return response;
    }
}
