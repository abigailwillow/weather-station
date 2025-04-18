using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WeatherStation.Services;

FunctionsApplicationBuilder builder = FunctionsApplication.CreateBuilder(args);

builder.Services.AddAzureClients(clientBuilder => {
    string? storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
    
    if (string.IsNullOrEmpty(storageConnectionString)) {
        throw new InvalidOperationException("AzureWebJobsStorage connection string is not configured.");
    }
    
    clientBuilder.AddBlobServiceClient(storageConnectionString);
    clientBuilder.AddQueueServiceClient(storageConnectionString);
    clientBuilder.AddTableServiceClient(storageConnectionString);
});

builder.Services.AddSingleton<IBlobProviderService, BlobProviderService>();
builder.Services.AddSingleton<IQueueProviderService, QueueProviderService>();
builder.Services.AddSingleton<ITableProviderService, TableProviderService>();

builder.Build().Run();
