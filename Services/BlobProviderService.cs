namespace WeatherStation.Services;

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

public class BlobProviderService : IBlobProviderService {
    private readonly BlobContainerClient _blob;

    public BlobProviderService(BlobServiceClient blobServiceClient) {
        _blob = blobServiceClient.GetBlobContainerClient("images");
        _blob.SetAccessPolicy(PublicAccessType.Blob);
        _blob.CreateIfNotExists();
    }

    public BlobContainerClient GetBlobContainerClient() => _blob;
}
