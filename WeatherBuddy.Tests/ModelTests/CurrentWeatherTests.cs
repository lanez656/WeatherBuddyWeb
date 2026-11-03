using System.Security.Cryptography.X509Certificates;
using WeatherBuddyApp.Models;
using Xunit; 

namespace WeatherBuddy.Tests.ModelTests;

public class CurrentWeatherTests
{
    [Fact]
    public void ConstructorSetsUpProperly()
    {
        int temperature = 10;
        double appTemp = 9.8;
        int weatherCode = 10;
        int humidity = 50;
        Wind wind = new(10, "Northwest");
        DateTime currentTime = new(2026, 11, 3);

        CurrentWeather currentWeather = new(temperature, appTemp, weatherCode, humidity, wind, currentTime);
        Assert.Equal(currentWeather.Temperature, temperature);
        Assert.Equal(currentWeather.ApparentTemperature, appTemp);
        Assert.Equal(currentWeather.WeatherCode, weatherCode);
        Assert.Equal(currentWeather.Humidity, humidity);
        Assert.Equal(currentWeather.Wind, wind);
        Assert.Equal(currentWeather.CurrentTime, currentTime);
    }
}

public class WindTests
{   
    [Theory]
    [InlineData(0.0, 0)]   
    [InlineData(2.5, 3)]
    
    [InlineData(4.5, 5)]
    [InlineData(4.4, 4)]
    // boundary testing
    [InlineData(4.6, 5)] 
    public void ConstructorRoundingWindSpeedCorrectly(double input, int expected)
    {   
        string windDirection = "N"; // fake wind direction here since we're only testing wind speed here
        var wind = new Wind(input, windDirection);
        Assert.Equal(expected, wind.WindSpeed);
    }
}