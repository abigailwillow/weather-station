namespace WeatherStation.Functions;

using System.Text.Json;

using Azure.Data.Tables;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

using WeatherStation.Models;
using WeatherStation.Entities;
using WeatherStation.Extensions;
using WeatherStation.Services;

public class SetupJob(ILoggerFactory loggerFactory, IQueueProviderService queueProviderService, ITableProviderService tableProviderService) {
    private const string WEATHER_API_URL = "https://data.buienradar.nl/2.0/feed/json";
    
    private readonly ILogger _logger = loggerFactory.CreateLogger<RequestImages>();
    private readonly QueueClient _imageGenerationQueue = queueProviderService.GetImageGenerationQueueClient();
    private readonly TableClient _jobsTable = tableProviderService.GetJobsTableClient();

    [Function(nameof(SetupJob))]
    public async Task Run([QueueTrigger("job-setup-queue")] QueueMessage message, FunctionContext context) {
        string jobId = message.MessageText;
        
        HttpClient client = new();
        HttpResponseMessage httpResponse = await client.GetAsync(WEATHER_API_URL);
        httpResponse.EnsureSuccessStatusCode();
        string jsonData = await httpResponse.Content.ReadAsStringAsync();
        JsonDocument weatherData = JsonDocument.Parse(jsonData);
        JsonElement measurementsData = weatherData.RootElement.GetProperty("actual").GetProperty("stationmeasurements");

        await _jobsTable.AddEntityAsync(new JobEntity {
            PartitionKey = "weatherJob",
            RowKey = jobId,
            
            StationCount = measurementsData.GetArrayLength()
        });
        
        int measurementIndex = 0;
        foreach (JsonElement measurementData in measurementsData.EnumerateArray()) {
            Measurement? measurement = measurementData.Deserialize<Measurement>();
            _imageGenerationQueue.SendMessageString(JsonSerializer.Serialize(new ImageGenerationRequest {
                JobId = jobId,
                MeasurementIndex = measurementIndex,
                StationName = measurement?.StationName ?? string.Empty,
                Temperature = measurement?.Temperature ?? 0.0f
            }));
            
            measurementIndex += 1;
        }
    }
}
