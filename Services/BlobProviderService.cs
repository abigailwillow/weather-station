namespace WeatherStation.Services;

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

public class BlobProviderService : IBlobProviderService {
    private readonly BlobContainerClient _imageBlob;

    public BlobProviderService(BlobServiceClient blobServiceClient) {
        _imageBlob = blobServiceClient.GetBlobContainerClient("images");
        _imageBlob.SetAccessPolicy(PublicAccessType.Blob);
        _imageBlob.CreateIfNotExists();
    }

    public BlobContainerClient GetImageBlobContainerClient() => _imageBlob;
}
