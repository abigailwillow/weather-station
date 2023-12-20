namespace WeatherStation.Models;

public class Measurement {
    public int stationid { get; set; }
    public string stationname { get; set; }
    public float temperature { get; set; }

    public Measurement(int id, string name, float temperature) => (this.stationid, this.stationname, this.temperature) = (id, name, temperature);
}
