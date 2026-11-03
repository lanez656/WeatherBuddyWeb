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
        Assert.Equal(temperature, currentWeather.Temperature);
        Assert.Equal(appTemp, currentWeather.ApparentTemperature);
        Assert.Equal(weatherCode, currentWeather.WeatherCode);
        Assert.Equal(humidity, currentWeather.Humidity);
        Assert.Same(wind, currentWeather.Wind);
        Assert.Equal(currentTime, currentWeather.CurrentTime);
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
        string fakeWindDirection = "N"; // fake wind direction here since we're only testing wind speed here
        var testWind = new Wind(input, fakeWindDirection);
        Assert.Equal(expected, testWind.WindSpeed);
    }
}