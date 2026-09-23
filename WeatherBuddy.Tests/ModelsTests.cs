using System.Collections.Generic;
using System.Reflection;
using WeatherBuddyApp.Models;
using WeatherBuddyApp.Models.Forecast;
using Xunit;

namespace WeatherBuddy.Tests;

public class ModelTests
{
    [Fact]
    public void Test_CurrentWeather_InstatiateCorrectly()
    {
        //Setup
        var currentWeather = new CurrentWeather(18, 1, 50, new Wind(3, "N"));

        //Assert
        Assert.Equal(18, currentWeather.Temperature);
        Assert.Equal(1, currentWeather.WeatherCode);
        Assert.Equal(50, currentWeather.Humidity);
        Assert.Equal(3, currentWeather.Wind.WindSpeed);
        Assert.Equal("N", currentWeather.Wind.Direction);
    }

    // rewrite test for dates


    [Fact]
    public void HourlyForecast_ShouldContain24HoursInCorrectOrder()

    {
        // Setup
        var hours = new List<Hour>();

        for (int i = 0; i < 24*7; i++)
        {
            hours.Add(new Hour(time: i, weatherCode: i + 1, temperature: i + 10, percipitation: i));
        }

        var forecast = new HourlyForecast(hours);

        // Assert
        Assert.Equal(24*7, forecast.HourlyForecasting.Count);

        for (int i = 0; i < 24*7; i++)
        {
            Assert.Equal(i, forecast.HourlyForecasting[i].Time);
            Assert.Equal(i + 1, forecast.HourlyForecasting[i].WeatherCode);
            Assert.Equal(i + 10, forecast.HourlyForecasting[i].Temperature);
            Assert.Equal(i, forecast.HourlyForecasting[i].Percipitation);
        }
    }
}