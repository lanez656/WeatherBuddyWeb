using WeatherBuddyApp.Controllers;
using WeatherBuddyApp.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using WeatherBuddyApp.Services;
using WeatherBuddyApp.Models.Forecast;
using Moq;

namespace WeatherBuddy.Tests;

public class ControllerTests
{
    private readonly Mock<IWeatherService> _mockService;
    private readonly HomeController _homeController;

    public ControllerTests()
    {
        _mockService = new Mock<IWeatherService>();
        _homeController = new HomeController(_mockService.Object);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Test_ControllerRedirectsWhenCityNameIsNullOrWhitespace(string? cityName)
    {
        CitySuggestion suggestion = new() {Name = cityName!};
        var result = await _homeController.Results(suggestion);
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
    }

    [Fact]
    public async Task Test_ControllerReturnsCityViewWhenCityIsFound()
    {
        CitySuggestion suggestion = new() {Name = "Esbjerg"};
        City expectedCity = CreateTestCity();
        _mockService
            .Setup(service => service.FetchCityDataAsync("Esbjerg"))
            .ReturnsAsync(expectedCity);

        var result = await _homeController.Results(suggestion);

        var viewResult = Assert.IsType<ViewResult>(result);
        var city = Assert.IsType<City>(viewResult.Model);
        Assert.Equal("Esbjerg", city.CityName);
        Assert.Same(expectedCity, city);
    }

    [Fact]
    public async Task Test_ControllerReturnsErrorViewWhenCityCannotBeFound()
    {
        CitySuggestion suggestion = new() {Name = "CityThatDefinitelyDoesNotExist123456"};
        _mockService
            .Setup(service => service.FetchCityDataAsync(suggestion.Name))
            .ThrowsAsync(new ArgumentException("City could not be found."));

        var result = await _homeController.Results(suggestion);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("An error occurred while fetching weather data.", _homeController.ViewBag.Error);
        Assert.Null(viewResult.Model);
    }


    [Fact]
    public void Test_ControllerDirectsToPrivacyWhenCalled()
    {
        var result = _homeController.Privacy();

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Null(viewResult.ViewName);
    }

    [Fact]
    public void Test_ControllerDirectsToIndexWhenCalled()
    {       
        var result = _homeController.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Null(viewResult.ViewName);
    }

    private static City CreateTestCity()
    {
        return new City(
            cityName: "Esbjerg",
            country: "Denmark",
            longitude: 8.45,
            latitude: 55.47,
            currentWeather: new CurrentWeather(12, 0, 50, new Wind(10, "NW")),
            hourlyForecast: new HourlyForecast([]),
            dailyForecast: new DailyForecast([]));
    }
}