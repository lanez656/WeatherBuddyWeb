using WeatherBuddyApp.Models.Forecast;

namespace WeatherBuddy.Tests.ModelTests.Forecast;

public class HourlyForecastTests
{
    private readonly List<Hour> _hours = [];
    private readonly HourlyForecast _hourlyForecast;
    
    public HourlyForecastTests()
    {
        for (int i = 0; i < 24*7; i++)
        {
            _hours.Add(new Hour(time: i, weatherCode: i + 1, temperature: i + 10, percipitation: i));
        }

        _hourlyForecast = new HourlyForecast(_hours);
    }
    
    [Fact]
    public void HourlyForecast_ShouldContain_24Hours_InCorrectOrder()
    {
        // Assert
        Assert.Equal(24*7, _hourlyForecast.HourlyForecasting.Count);

        for (int i = 0; i < 24*7; i++)
        {
            Assert.Equal(i, _hourlyForecast.HourlyForecasting[i].Time);
            Assert.Equal(i + 1, _hourlyForecast.HourlyForecasting[i].WeatherCode);
            Assert.Equal(i + 10, _hourlyForecast.HourlyForecasting[i].Temperature);
            Assert.Equal(i, _hourlyForecast.HourlyForecasting[i].Percipitation);
        }
    }
    
    [Fact]
    public void ToString_NoHours_ReturnsNoDataMessage()
    {
        HourlyForecast emptyForecast = new([]);

        Assert.Equal("Hourly Forecast: No data available", emptyForecast.ToString());
    }

    [Fact]
    public void ToString_StartsWithHeader()
    {
        Assert.StartsWith("=== HOURLY FORECAST ===", _hourlyForecast.ToString());
    }
}