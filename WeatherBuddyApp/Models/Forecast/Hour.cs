using WeatherBuddyApp.Services;
namespace WeatherBuddyApp.Models.Forecast;

public class Hour(int time, int weatherCode, double temperature, int percipitation)
{
    public int Time {get; set;} = time;
    public int WeatherCode {get; set;} = weatherCode;
    public double Temperature {get; set;} = temperature;
    public int Percipitation {get; set;} = percipitation;
    public override string ToString()
    {
        // Formats hour to 2 digits (e.g. 09:00, 14:00)
        return $"{Time:D2}:00 | Temp: {Temperature}°C | Precip: {Percipitation}% | {WeatherCodeDictionary.TranslateCode(WeatherCode)}";
    }
}