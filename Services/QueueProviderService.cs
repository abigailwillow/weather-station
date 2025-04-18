namespace WeatherStation.Services;

using Azure.Storage.Queues;

public class QueueProviderService : IQueueProviderService {
    private readonly QueueClient _jobSetupQueue;
    private readonly QueueClient _imageGenerationQueue;

    public QueueProviderService(QueueServiceClient queueServiceClient) {
        _jobSetupQueue = queueServiceClient.GetQueueClient("job-setup-queue");
        _jobSetupQueue.CreateIfNotExists();

        _imageGenerationQueue = queueServiceClient.GetQueueClient("image-generation-queue");
        _imageGenerationQueue.CreateIfNotExists();
    }

    public QueueClient GetJobSetupQueueClient() => _jobSetupQueue;
    public QueueClient GetImageGenerationQueueClient() => _imageGenerationQueue;
}
