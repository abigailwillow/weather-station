namespace WeatherStation.Functions;

using System.Text.Json;

using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

using WeatherStation.Entities;
using WeatherStation.Models;
using WeatherStation.Services;

public class Jobs(ILoggerFactory loggerFactory,  IQueueProviderService queueProviderService, IBlobProviderService blobProviderService, ITableProviderService tableProviderService) {
    private readonly ILogger _logger = loggerFactory.CreateLogger<Jobs>();
    private readonly QueueClient _queue = queueProviderService.GetImageGenerationQueueClient();
    private readonly BlobContainerClient _blob = blobProviderService.GetImageBlobContainerClient();
    private readonly TableClient _jobsTable = tableProviderService.GetJobsTableClient();
    
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
    
    private async Task<int> GetImageCount(string jobId) {
        JobEntity? jobEntity = await _jobsTable.GetEntityAsync<JobEntity>("weatherJob", jobId);

        return jobEntity?.StationCount ?? 0;
    }
    
    [Function(nameof(Jobs))]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "jobs/{jobId}")] HttpRequestData request, string jobId) {
        _logger.LogInformation("User tried to get job {JobId}", jobId);
        
        JobsHttpResponse response = new() { Images = [] };

        int imageCount = await GetImageCount(jobId);
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
