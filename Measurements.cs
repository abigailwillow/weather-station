namespace WeatherStation;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using WeatherStation.Models;
using WeatherStation.Utilities;

public class Measurements {
    private readonly ILogger log;

    public Measurements(ILoggerFactory loggerFactory) {
        log = loggerFactory.CreateLogger<Measurements>();
    }

    [Function("Measurements")]
    public async Task<HttpResponseData> GetMeasurements([HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "measurements")] HttpRequestData request) {
        const string URL = "https://data.buienradar.nl/2.0/feed/json";

        HttpResponseData response = request.CreateResponse();

        string json;
        HttpClient client = new();
        HttpResponseMessage httpResponse = await client.GetAsync(URL);
        httpResponse.EnsureSuccessStatusCode();
        json =  await httpResponse.Content.ReadAsStringAsync();

        JObject weatherData = JObject.Parse(json);
        List<JToken> measurementsData = weatherData["actual"]["stationmeasurements"].Children().ToList();
        List<Measurement> measurements = new List<Measurement>();
        foreach (JToken measurementData in measurementsData) {
            Measurement measurement = measurementData.ToObject<Measurement>();
            measurements.Add(measurement);
        }

        string firstStationName = measurements[0].stationname;
        string firstTemperature = $"{measurements[0].temperature}°C";

        byte[] image = await GetRandomImage();
        image = ImageHelper.AddTextToImage(image, (firstStationName, (16, 16), 32, "#FFFFFF"));
        image = ImageHelper.AddTextToImage(image, (firstTemperature, (16, 48), 32, "#FFFFFF"));

        await response.WriteBytesAsync(image);

        return response;
    }

    private static async Task<byte[]> GetRandomImage() {
        const string UNSPLASH_URL = "https://source.unsplash.com/random";

        HttpClient client = new();
        HttpResponseMessage response = await client.GetAsync(UNSPLASH_URL);
        return await response.Content.ReadAsByteArrayAsync();
    }
}
