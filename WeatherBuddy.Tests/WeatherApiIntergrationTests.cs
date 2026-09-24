using System.Threading.Tasks;
using WeatherBuddyApp.Models;
using WeatherBuddyApp.Models.Forecast;
using WeatherBuddyApp.Services;
using Xunit;

namespace WeatherBuddy.Tests;

public class WeatherApiIntegrationTests
{
    [Fact]
    public async Task FetchCityDataAsync_FromOpenMeteoApi_InstantiatesCityCorrectly()
    {
        // Arrange
        string cityName = "Esbjerg";

        // Act
        City city = await WeatherApi.FetchCityDataAsync(cityName);

        // Assert
        Assert.NotNull(city);

        // Verify Current Weather
        Assert.NotNull(city.CurrentWeather);

        // Verify Hourly Forecast & nested Hours
        Assert.NotNull(city.HourlyForecast);
        Assert.NotEmpty(city.HourlyForecast.HourlyForecasting);
        Assert.Equal(168, city.HourlyForecast.HourlyForecasting.Count);

        // Verify Daily Forecast & nested Days
        Assert.NotNull(city.DailyForecast);
        Assert.NotEmpty(city.DailyForecast.DailyForecasting);
        Assert.Equal(7, city.DailyForecast.DailyForecasting.Count);
    }
}