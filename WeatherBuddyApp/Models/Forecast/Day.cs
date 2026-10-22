using Microsoft.AspNetCore.SignalR;
using WeatherBuddyApp.Services;
namespace WeatherBuddyApp.Models.Forecast;

public class Day(string date, int weatherCode, double tempMax, double tempMin, DateTime sunrise, DateTime sunset)
{
    public string Date {get; set;} = date;
    public int WeatherCode {get; set;} = weatherCode;
    public double TempMax {get; set;} = tempMax;
    public double TempMin {get; set;} = tempMin;

    public DateTime Sunrise {get; set;} = sunrise;

    public DateTime Sunset {get; set;} = sunset;

    public override string ToString()
    {
        return $"{Date} | {WeatherCodeDictionary.TranslateCode(WeatherCode)} | Maximun Temperature {TempMax} | Minimum Temperature {TempMin}";
    }

    public string GetSunriseAndSunset()
    {
        return $"Sunrise: {Sunrise:HH:mm} | Sunset: {Sunset:HH:mm}";
    }

}