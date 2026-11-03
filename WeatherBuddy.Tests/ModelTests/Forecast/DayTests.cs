using WeatherBuddyApp.Models.Forecast;

namespace WeatherBuddy.Tests.ModelTests.Forecast;

public class DayTests{
    private readonly string _date = "2026-11-03";
    private readonly int _weatherCode = 61;
    private readonly int _tempMax = 14; 
    private readonly int _tempMin = 8;
    private readonly DateTime _sunrise = new(2026, 11, 3, 7, 19, 0);
    private readonly DateTime _sunset = new(2026, 11, 3, 16, 12, 0);
    private readonly Day _testDay;

    public DayTests()
    {
        _testDay = new(_date, _weatherCode, _tempMax, _tempMin, _sunrise, _sunset);
    }

    [Fact]
    public void Test_Day_Instatiate_Correctly()
    {
        Assert.Equal(_date, _testDay.Date);
        Assert.Equal(_weatherCode, _testDay.WeatherCode);
        Assert.Equal(_tempMax, _testDay.TempMax);
        Assert.Equal(_tempMin, _testDay.TempMin);
        Assert.Equal(_sunrise, _testDay.Sunrise);
        Assert.Equal(_sunset, _testDay.Sunset);
    }

    [Fact]
    public void ToString_ShowsDateWeatherAndTemperatures()
    {
        Assert.Equal("2026-11-03 | Light Rain | Maximum Temperature 14 | Minimum Temperature 8", _testDay.ToString());
    }

    [Fact]
    public void GetSunriseAndSunset_ShowsTimesInHoursAndMinutes()
    {
        Assert.Equal("Sunrise: 07.19 | Sunset: 16.12", _testDay.GetSunriseAndSunset());
    }

}