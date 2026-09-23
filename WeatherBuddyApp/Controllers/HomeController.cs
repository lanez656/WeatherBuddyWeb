using System.Diagnostics;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using WeatherBuddyApp.Models;
using WeatherBuddyApp.Services;

namespace WeatherBuddyApp.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Results(string? cityName)
    {
        if (string.IsNullOrWhiteSpace(cityName))
        {
            // Redirect back to home if user submits an empty search
            return RedirectToAction("Index");
        }

        try
        {
            City resultCity = await WeatherApi.FetchCityDataAsync(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(cityName));
            return View(resultCity);
        }
        catch (Exception)
        {
            ViewBag.Error = "An error occurred while fetching weather data.";
            return View();
        }
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
