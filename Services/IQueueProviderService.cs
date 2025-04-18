namespace WeatherStation.Services;

using Azure.Storage.Queues;

public interface IQueueProviderService {
    public QueueClient GetQueueClient();
}
