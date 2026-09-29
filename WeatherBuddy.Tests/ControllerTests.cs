using WeatherBuddyApp.Controllers;
using WeatherBuddyApp.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace WeatherBuddy.Tests;

public class ControllerTests
{

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Test_ControllerRedirectsWhenCityNameIsNullOrWhitespace(string? cityName)
    {
        HomeController controller = new();
        CitySuggestion suggestion = new() {Name = cityName!};

        var result = await controller.Results(suggestion);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
    }

    [Fact]
    public async Task Test_ControllerReturnsCityViewWhenCityIsFound()
    {
        HomeController controller = new();
        CitySuggestion suggestion = new() {Name = "Esbjerg"};

        var result = await controller.Results(suggestion);

        var viewResult = Assert.IsType<ViewResult>(result);
        var city = Assert.IsType<City>(viewResult.Model);
        Assert.Equal("Esbjerg", city.CityName);
    }

    [Fact]
    public async Task Test_ControllerReturnsErrorViewWhenCityCannotBeFound()
    {
        HomeController controller = new();
        CitySuggestion suggestion = new() {Name = "CityThatDefinitelyDoesNotExist123456"};

        var result = await controller.Results(suggestion);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("An error occurred while fetching weather data.", controller.ViewBag.Error);
        Assert.Null(viewResult.Model);
    }


    [Fact]
    public void Test_ControllerDirectsToPrivacyWhenCalled()
    {
        HomeController controller = new();

        var result = controller.Privacy();

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Null(viewResult.ViewName);
    }

    [Fact]
    public void Test_ControllerDirectsToIndexWhenCalled()
    {
        HomeController controller = new();

        var result = controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Null(viewResult.ViewName);
    }
}