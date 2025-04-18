namespace WeatherStation.Functions;

using System.Text.Json;

using Azure.Data.Tables;
using Azure.Storage.Queues;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

using WeatherStation.Models;
using WeatherStation.Extensions;
using WeatherStation.Services;

public class RequestImages(ILoggerFactory loggerFactory, IQueueProviderService queueProviderService, ITableProviderService tableProviderService) {
    private const string WEATHER_API_URL = "https://data.buienradar.nl/2.0/feed/json";
    
    private readonly ILogger _logger = loggerFactory.CreateLogger<RequestImages>();
    private readonly QueueClient _jobSetupQueue = queueProviderService.GetJobSetupQueueClient();
    private readonly TableClient _jobsTable = tableProviderService.GetJobsTableClient();

    [Function(nameof(RequestImages))]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "request-images")] HttpRequestData request) {
        string jobId = Guid.NewGuid().ToString();
        string jobUrl = $"{request.Url.GetLeftPart(UriPartial.Authority)}/api/jobs/{jobId}";

        _jobSetupQueue.SendMessageString(jobId);

        HttpResponseData response = request.CreateResponse();
        await response.WriteStringAsync(JsonSerializer.Serialize(new ImageGenerationHttpResponse {
            JobId = jobId,
            JobUrl = jobUrl
        }));
        return response;
    }
}
