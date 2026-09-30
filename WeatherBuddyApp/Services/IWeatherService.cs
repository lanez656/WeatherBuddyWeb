using WeatherBuddyApp.Models;

namespace WeatherBuddyApp.Services;

public interface IWeatherService
{
    Task<City> FetchCityDataAsync(string cityName);
    Task<City> FetchCityDataAsync(CitySuggestion suggestion);
    Task<List<CitySuggestion>> GetCitySuggestionsAsync(string query);
}