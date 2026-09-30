using System.Diagnostics;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using WeatherBuddyApp.Models;
using WeatherBuddyApp.Services;

namespace WeatherBuddyApp.Controllers;

public class HomeController(IWeatherService weatherService) : Controller 
{
    private readonly IWeatherService _weatherService = weatherService;

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Results(CitySuggestion suggestion)
    {
        if (suggestion == null || string.IsNullOrWhiteSpace(suggestion.Name))
        {
            return RedirectToAction("Index");
        }

        try
        {
            City resultCity = suggestion.Latitude == 0 && suggestion.Longitude == 0
                ? await _weatherService.FetchCityDataAsync(suggestion.Name)
                : await _weatherService.FetchCityDataAsync(suggestion);

            return View(resultCity);
        }
        catch (Exception)
        {
            ViewBag.Error = "An error occurred while fetching weather data.";
            return View();
        }
    }

    [HttpGet]
    public async Task<IActionResult> Suggestions(string query)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
        {
            return Json(Array.Empty<CitySuggestion>());
        }
        
        var suggestions = await _weatherService.GetCitySuggestionsAsync(query);
        return Json(suggestions);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
