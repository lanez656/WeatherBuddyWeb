using Microsoft.AspNetCore.Mvc;
using WeatherBuddyApp.Controllers;
using WeatherBuddyApp.Models;
using WeatherBuddyApp.Models.Forecast;
using WeatherBuddyApp.Services;
using Moq;


namespace WeatherBuddy.Tests;

public class HomeControllerTests
{
    private readonly Mock<IWeatherService> _mockService;
    private readonly HomeController _homeController;

    public HomeControllerTests()
    {
        _mockService = new Mock<IWeatherService>();
        _homeController = new HomeController(_mockService.Object);
    }
    
    [Fact]
    public void Index_Returns_View()
    {
        HomeController homeController = new(null!);
        var testView = homeController.Index();
        Assert.IsType<ViewResult>(testView);
    }

   
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("a")]
    public async Task Suggestions_TooShortQuery_ReturnsEmptyList(string? query)
    {
        var result = await _homeController.Suggestions(query!);

        var jsonResult = Assert.IsType<JsonResult>(result);
        var suggestions = Assert.IsType<CitySuggestion[]>(jsonResult.Value);
        Assert.Empty(suggestions);
        _mockService
            .Verify(service => service.GetCitySuggestionsAsync(It.IsAny<string>()), Times.Never);
            // Verify asks the mock afterwards what happened: 
            // was GetCitySuggestionsAsync called with any string? It should be zero times.
    }

    [Theory]
    [InlineData("Od")]       // exactly 2 characters so this is the boundary
    [InlineData("Odense")]   // the normal query if they type out the whole cityname
    public async Task Suggestions_ValidQuery_ReturnsSuggestions(string query)
    {
        List<CitySuggestion> expectedSuggestions = [new() { Name = "Odense", Country = "Denmark" }];
        _mockService
            .Setup(service => service.GetCitySuggestionsAsync(query))
            .ReturnsAsync(expectedSuggestions);

        var result = await _homeController.Suggestions(query);

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.Same(expectedSuggestions, jsonResult.Value);
    }

    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Results_Returns_ErrorView_When_CitySuggestionIsNullOrWhiteSpace(string? cityName)
    {
        CitySuggestion suggestion = new() {Name = cityName!};
        var result = await _homeController.Results(suggestion);
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
    }
    
    [Fact]
    public async Task Results_TypedCityNameAndPressedEnter_ShowsCity()
    {
        CitySuggestion suggestion = new() {Name = "Esbjerg"};
        City expectedCity = CreateTestCity();
        _mockService
            .Setup(service => service.FetchCityDataAsync("Esbjerg"))
            .ReturnsAsync(expectedCity);

        var result = await _homeController.Results(suggestion);

        var viewResult = Assert.IsType<ViewResult>(result);
        var city = Assert.IsType<City>(viewResult.Model);
        Assert.Same(expectedCity, city);
    }

    [Fact]
    public async Task Results_ClickedCitySuggestion_ShowsCity()
    {
        CitySuggestion suggestion = new() {Name = "Esbjerg", Latitude = 55.47, Longitude = 8.45};
        City expectedCity = CreateTestCity();
        _mockService
            .Setup(service => service.FetchCityDataAsync(suggestion))
            .ReturnsAsync(expectedCity);
        
        var result = await _homeController.Results(suggestion);

        var viewResult = Assert.IsType<ViewResult>(result);
        var city = Assert.IsType<City>(viewResult.Model);
        Assert.Same(expectedCity, city);
    }

    [Fact]
    public async Task Results_ReturnsErrorView_WhenCityCannotBeFound()
    {
        CitySuggestion suggestion = new() {Name = "CityThatDefinitelyDoesNotExist123456"};
        string errorMessage = "An error occurred while fetching weather data.";
        _mockService
            .Setup(service => service.FetchCityDataAsync(suggestion.Name))
            .ThrowsAsync(new ArgumentException("City could not be found."));

        var result = await _homeController.Results(suggestion);
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(errorMessage, _homeController.ViewBag.Error);
        Assert.Null(viewResult.Model);
    }
    
    private static City CreateTestCity()
    {
        return new City(
            cityName: "Esbjerg",
            country: "Denmark",
            longitude: 8.45,
            latitude: 55.47,
            currentWeather: new CurrentWeather(12, 14, 0, 50, new Wind(10, "Northwest"), DateTime.Now),
            hourlyForecast: new HourlyForecast([]),
            dailyForecast: new DailyForecast([]));
    }
}