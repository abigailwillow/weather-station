namespace WeatherStation;

using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using WeatherStation.Models;

public class Measurements {
    private readonly ILogger log;

    public Measurements(ILoggerFactory loggerFactory) {
        log = loggerFactory.CreateLogger<Measurements>();
    }

    [Function("Measurements")]
    public async Task<HttpResponseData> GetMeasurements([HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "measurements")] HttpRequestData request) {
        const string URL = "https://data.buienradar.nl/2.0/feed/json";

        HttpResponseData response = request.CreateResponse();

        string json = string.Empty;
        try {
            HttpClient client = new();
            HttpResponseMessage httpResponse = await client.GetAsync(URL);
            httpResponse.EnsureSuccessStatusCode();
            json =  await httpResponse.Content.ReadAsStringAsync();
        } catch (Exception exception) {
            log.LogError(exception.Message);
            response.StatusCode = HttpStatusCode.InternalServerError;
            return response;
        }

        JObject weatherData = JObject.Parse(json);
        List<JToken> measurementsData = weatherData["actual"]["stationmeasurements"].Children().ToList();
        List<Measurement> measurements = new List<Measurement>();
        foreach (JToken measurementData in measurementsData) {
            Measurement measurement = measurementData.ToObject<Measurement>();
            measurements.Add(measurement);
        }

        await response.WriteAsJsonAsync(measurements);

        return response;
    }
}
