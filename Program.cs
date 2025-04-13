using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Hosting;

FunctionsApplicationBuilder builder = FunctionsApplication.CreateBuilder(args);

builder.Services.AddAzureClients(clientBuilder => {
    string? storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
    
    if (string.IsNullOrEmpty(storageConnectionString)) {
        throw new InvalidOperationException("AzureWebJobsStorage connection string is not configured.");
    }
    
    clientBuilder.AddBlobServiceClient(storageConnectionString);
    clientBuilder.AddQueueServiceClient(storageConnectionString);
});

builder.Build().Run();
