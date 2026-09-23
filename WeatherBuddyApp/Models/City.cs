
using WeatherBuddyApp.Models;
using WeatherBuddyApp.Models.Forecast;
using WeatherBuddyApp.Services;

namespace WeatherBuddyApp.Models;

public class City(string cityName, string country, CurrentWeather currentWeather, HourlyForecast hourlyForecast, DailyForecast dailyForecast)
{
    public string CityName {get; set;} = cityName;
    public string Country {get; set;} = country;
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
}