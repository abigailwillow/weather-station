namespace WeatherStation.Services;

using Azure.Data.Tables;

public interface ITableProviderService {
    public TableClient GetJobsTableClient();
}
