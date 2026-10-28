namespace WeatherBuddyApp.Models;

public class CurrentWeather (double temperature, double apparentTemperature, int weatherCode, int humidity, Wind wind, DateTime currentTime)
{
    public double Temperature {get; set;} = temperature;
    public double ApparentTemperature { get; set;} = apparentTemperature;
    public int WeatherCode {get; set;} = weatherCode;
    public int Humidity { get; set; } = humidity;
    public Wind Wind { get; set; } = wind;
    public DateTime CurrentTime {get; set;} = currentTime;
}

public class Wind(double windSpeed, string direction)
{
    public double WindSpeed = windSpeed;
    public string Direction= direction;
}