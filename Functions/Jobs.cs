using WeatherStation.Services;

namespace WeatherStation.Functions;

using System.Text.Json;

using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

using WeatherStation.Models;

public class Jobs(ILoggerFactory loggerFactory,  IQueueProviderService queueProviderService, IBlobProviderService blobProviderService) {
    private readonly ILogger _logger = loggerFactory.CreateLogger<Jobs>();
    private readonly QueueClient _queue = queueProviderService.GetQueueClient();
    private readonly BlobContainerClient _blob = blobProviderService.GetBlobContainerClient();

    private bool IsValidImage(string jobId, int measurementIndex) {
        BlobClient imageBlobClient = _blob.GetBlobClient($"{jobId}-{measurementIndex}.png");
        return imageBlobClient.Exists();
    }
    
    private int GetValidImageCount(string jobId) {
        int imageCount = 0;

        while (IsValidImage(jobId, imageCount)) {
            imageCount++;
        };

        return imageCount;
    }
    
    private int GetImageCount(string jobId) {
        BlobClient metaBlobClient = _blob.GetBlobClient($"{jobId}.json");
        if (!metaBlobClient.Exists()) return 0;
        
        string jsonString = metaBlobClient.DownloadContent().Value.Content.ToString();
        JobMetadata? metadata = JsonSerializer.Deserialize<JobMetadata>(jsonString);
        return metadata?.StationCount ?? 0;
    }
    
    [Function(nameof(Jobs))]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "jobs/{jobId}")] HttpRequestData request, string jobId) {
        _logger.LogInformation("User tried to get job {JobID}", jobId);
        
        JobsHttpResponse response = new() { Images = [] };

        int imageCount = GetImageCount(jobId);
        int validImageCount = GetValidImageCount(jobId);
        
        if (validImageCount > 0) {
            response.Status = validImageCount < imageCount ? JobStatus.PENDING : JobStatus.COMPLETED;

            for (int measurementIndex = 0; measurementIndex < validImageCount; measurementIndex++) {
                BlobClient imageBlobClient = _blob.GetBlobClient($"{jobId}-{measurementIndex}.png");
                string imageUrl = imageBlobClient.Uri.ToString();
                response.Images.Add(imageUrl);
            }
        } else {
            response.Status = imageCount > 0 ? JobStatus.PENDING : JobStatus.INVALID;
        }

        HttpResponseData responseData = request.CreateResponse();
        await responseData.WriteStringAsync(JsonSerializer.Serialize(response));
        return responseData;
    }
}
