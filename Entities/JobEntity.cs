namespace WeatherStation.Entities;

using System.Text.Json.Serialization;
using Azure;
using Azure.Data.Tables;

public class JobEntity : ITableEntity {
    public required int StationCount { get; set; }
    
    public required string PartitionKey { get; set; }
    public required string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}
