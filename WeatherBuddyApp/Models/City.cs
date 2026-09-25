
using WeatherBuddyApp.Models;
using WeatherBuddyApp.Models.Forecast;
using WeatherBuddyApp.Services;

namespace WeatherBuddyApp.Models;

public class City(string cityName, string country, double longitude, double latitude, CurrentWeather currentWeather, HourlyForecast hourlyForecast, DailyForecast dailyForecast)
{
    public string CityName {get; set;} = cityName;
    public string Country {get; set;} = country;
    public string StateOrRegion { get; set; } = "";
    public double Longitude {get; set;} = longitude;

    public double Latitude {get; set;} = latitude;
    public CurrentWeather  CurrentWeather {get; set;} = currentWeather;
    public HourlyForecast HourlyForecast {get; set;} = hourlyForecast;
    public DailyForecast DailyForecast {get;set;} = dailyForecast;


    public override string ToString()
    {
        return $"City: {CityName}, {Country}\n" +
               $"Current Temp: {CurrentWeather?.Temperature}°C, Weather: {WeatherCodeDictionary.TranslateCode(CurrentWeather!.WeatherCode)}\n" +
               $"{HourlyForecast.ToString()}\n" +
               $"{DailyForecast.ToString()}\n";
    }

    public string GetFormattedCoordinates()
    {
        char latDir = Latitude >= 0 ? 'N' : 'S';
        char lonDir = Longitude >= 0 ? 'E' : 'W'; // Ret til 'Ø' / 'V' hvis sproget skal være dansk

        return $"{Math.Abs(Latitude):F2}° {latDir}, {Math.Abs(Longitude):F2}° {lonDir}";
    }
}