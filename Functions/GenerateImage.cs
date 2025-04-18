namespace WeatherStation.Functions;

using System.Text.Json;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using WeatherStation.Models;
using WeatherStation.Utilities;
using WeatherStation.Services;

public class GenerateImage(ILoggerFactory loggerFactory, IBlobProviderService blobProviderService) {
    private const string IMAGE_API_URL = "https://picsum.photos/1024/1024";

    private readonly ILogger _logger = loggerFactory.CreateLogger<GenerateImage>();
    private readonly BlobContainerClient _blob = blobProviderService.GetBlobContainerClient();

    [Function(nameof(GenerateImage))]
    public async Task Run([QueueTrigger("image-generation-queue")] QueueMessage message, FunctionContext context) {
        JsonElement jsonDocument = JsonDocument.Parse(message.MessageText).RootElement;
        ImageGenerationRequest imageGenerationRequest = jsonDocument.Deserialize<ImageGenerationRequest>();

        _logger.LogInformation("Started job {JobId} for station {StationName}.", imageGenerationRequest.JobId,
            imageGenerationRequest.StationName);

        _logger.LogInformation("Station: {StationName}, Temperature: {Temperature}°C",
            imageGenerationRequest.StationName, imageGenerationRequest.Temperature);

        byte[] image = await GetRandomImage();
        image = ImageUtility.AddTextToImage(image, (imageGenerationRequest.StationName, (16, 16), 32, "#FFFFFF"));
        image = ImageUtility.AddTextToImage(image,
            ($"{imageGenerationRequest.Temperature}°C", (16, 48), 32, "#FFFFFF"));

        using MemoryStream memoryStream = new(image);

        BlobClient imageBlobClient =
            _blob.GetBlobClient($"{imageGenerationRequest.JobId}-{imageGenerationRequest.MeasurementIndex}.png");
        await imageBlobClient.UploadAsync(
            memoryStream,
            new BlobHttpHeaders {
                ContentType = "image/png"
            }
        );
    }

    private static async Task<byte[]> GetRandomImage() {
        HttpClient client = new();
        HttpResponseMessage response = await client.GetAsync(IMAGE_API_URL);
        return await response.Content.ReadAsByteArrayAsync();
    }
}
