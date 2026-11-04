using WeatherBuddyApp.Models.Forecast;

namespace WeatherBuddy.Tests.ModelTests.Forecast;

public class DailyForecastTests
{
    private readonly List<Day> _days = [];
    private readonly DailyForecast _testDailyForecast;

    public DailyForecastTests()
    {
        for (int i = 0; i < 7; i++)
        {
            DateTime date = new DateTime(2026, 11, 3).AddDays(i);
            _days.Add(new Day(
                date: date.ToString("yyyy-MM-dd"),
                weatherCode: 61,
                tempMax: 14 + i,
                tempMin: 8 + i,
                sunrise: date.AddHours(7),
                sunset: date.AddHours(16)));
        }

        _testDailyForecast = new DailyForecast(_days);
    }

    [Fact]
    public void DailyForecast_Instantiates_Correctly()
    {
        Assert.Same(_days, _testDailyForecast.DailyForecasting);
    }

    [Fact]
    public void ToString_NoDays_ReturnsNoDataMessage()
    {
        DailyForecast emptyForecast =  new([]);

        Assert.Equal("Daily Forecast: No data available", emptyForecast.ToString());
    }

    [Fact]
    public void ToString_StartsWithHeader()
    {
        Assert.StartsWith("=== DAILY FORECAST ===", _testDailyForecast.ToString());
    }

    [Fact]
    public void ToString_ShowsAllDays()
    {
        string[] lines = _testDailyForecast.ToString().Split(Environment.NewLine);

        Assert.Equal(8, lines.Length); // 1 header + 7 days
    }

    [Fact]
    public void ToString_ShowsDaysInOrderWithIndent()
    {
        string[] lines = _testDailyForecast.ToString().Split(Environment.NewLine);

        for (int i = 0; i < _days.Count; i++)
        {
            Assert.Equal($"  {_days[i]}", lines[i + 1]); // + 1 to skip  header
        }
    }
}