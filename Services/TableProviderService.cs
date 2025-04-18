namespace WeatherStation.Services;

using Azure.Data.Tables;

public class TableProviderService : ITableProviderService{
    private readonly TableClient _jobsTable;

    public TableProviderService(TableServiceClient tableServiceClient) {
        _jobsTable = tableServiceClient.GetTableClient("jobs");
        _jobsTable.CreateIfNotExists();
    }

    public TableClient GetJobsTableClient() => _jobsTable;
}
