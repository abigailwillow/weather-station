namespace WeatherStation.Services;

using Azure.Storage.Queues;

public class QueueProviderService : IQueueProviderService
{
    private readonly QueueClient _queue;

    public QueueProviderService(QueueServiceClient queueServiceClient) {
        _queue = queueServiceClient.GetQueueClient("image-generation-queue");
        _queue.CreateIfNotExists();
    }

    public QueueClient GetQueueClient() => _queue;
}