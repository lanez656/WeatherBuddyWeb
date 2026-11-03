using WeatherBuddyApp.Models;
using WeatherBuddyApp.Models.Forecast;

namespace WeatherBuddy.Tests.ModelTests;

public class CityTests
{
    private readonly string _cityName = "Odense";
    private readonly string _country = "Denmark";
    private readonly double _longitude = 10.38831;
    private readonly double _latitude = 55.39594;
    private readonly CurrentWeather _currentWeather;
    private readonly HourlyForecast _hourlyForecast;
    private readonly DailyForecast _dailyForecast;
    private readonly City _testCity;


    public CityTests()
    {
        _currentWeather = new CurrentWeather(12, 9.8, 61, 70, new Wind(5, "NW"), new DateTime(2026, 11, 3, 14, 0, 0));
        _hourlyForecast = new HourlyForecast([]);
        _dailyForecast = new DailyForecast([]);

        _testCity = new City(_cityName, _country, _longitude, _latitude,
                             _currentWeather, _hourlyForecast, _dailyForecast);
    }   
    [Fact]
    public void Constructor_SetsUpProperly()
    {
        Assert.Equal(_cityName, _testCity.CityName);
        Assert.Equal(_country, _testCity.Country);
        Assert.Equal("", _testCity.StateOrRegion);
        Assert.Equal(_longitude, _testCity.Longitude);
        Assert.Equal(_latitude, _testCity.Latitude);
        Assert.Same(_currentWeather, _testCity.CurrentWeather);
        Assert.Same(_hourlyForecast, _testCity.HourlyForecast);
        Assert.Same(_dailyForecast, _testCity.DailyForecast);
    }

    [Fact]
    public void ToString_PrintsCorrectly()
    {
        string testString = _testCity.ToString();
        string expectedString = "City: Odense, Denmark\nCurrent Temp: 12°C, Weather: Light Rain\nHourly Forecast: No data available\nDaily Forecast: No data available\n";
        Assert.Equal(expectedString, testString);
    } // OBS: this test relies on WeatherCodeDictionary so if we change that this might fail

    [Theory]
    [InlineData(55.67, 12.57, "N", "E")]
    [InlineData(40.71, -74.01, "N", "W")]
    [InlineData(-33.87, 151.21, "S", "E")]
    [InlineData(-34.60, -58.38, "S", "W")]
    public void GetFormattedCoordinates_ShowsCorrectDirections(double lat, double lon, string latDir, string lonDir)
    {
        _testCity.Latitude = lat;
        _testCity.Longitude = lon;

        string result = _testCity.GetFormattedCoordinates();

        Assert.Contains($"° {latDir},", result); // check that latitude is in the string
        Assert.EndsWith($"° {lonDir}", result); // check that the string ends with longitude, and that its correct
    }    
}