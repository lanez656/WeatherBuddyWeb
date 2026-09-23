namespace WeatherBuddyApp.Models;

public class CurrentWeather (double temperature, int weatherCode, int humidity, Wind wind)
{
    public double Temperature {get; set;} = temperature;
    public int WeatherCode {get; set;} = weatherCode;
    public int Humidity { get; set; } = humidity;
    public Wind Wind { get; set; } = wind;
}

public class Wind(int windSpeed, string direction)
{
    public int WindSpeed = windSpeed;
    public string Direction= direction;
}