namespace WeatherStation.Models;

public class Measurement {
    public int Id;
    public string Name;
    public float Latitude;
    public float Longitude;
    public string Region;
    public DateTime Time;
    public string Description;
    public string IconURL;
    public string FullIconURL;
    public string GraphURL;
    public string WindDirection;
    public float Temperature;
    public float GroundTemperature;
    public float FeelTemperature;
    public float WindGusts;
    public float WindSpeed;
    public int WindSpeedBeaufort;
    public int Humidity;
    public float Percipitation;
    public float SunPower;
    public float RainFall24Hour;
    public float RainFall1Hour;
    public int WindDirectionDegrees;
}
