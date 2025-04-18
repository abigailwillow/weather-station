namespace WeatherStation.Services;

using Azure.Storage.Queues;

public interface IQueueProviderService {
    public QueueClient GetJobSetupQueueClient();
    public QueueClient GetImageGenerationQueueClient();
}
