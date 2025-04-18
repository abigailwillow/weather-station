namespace WeatherStation.Services;

using Azure.Storage.Blobs;

public interface IBlobProviderService {
    public BlobContainerClient GetBlobContainerClient();
}
