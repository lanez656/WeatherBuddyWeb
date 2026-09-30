using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using System.Threading.Tasks;
using WeatherBuddyApp.Models;
using WeatherBuddyApp.Models.Forecast;
using WeatherBuddyApp.Services;

namespace WeatherBuddyApp.Services;


public class WeatherService(HttpClient httpClient) : IWeatherService
{

    private readonly HttpClient _httpClient = httpClient;
    /// <summary> Fetches the weather data for a given <see cref="CitySuggestion"/> </summary>
    /// <param name="suggestion">the given city we want to find the weather data for</param>
    /// <returns>A <see cref="City"/> with current, hourly and daily weather.</returns>
    public async Task<City> FetchCityDataAsync(CitySuggestion suggestion)
    {
        string url = ForecastUrlBuilder(suggestion);
        string jsonString = await _httpClient.GetStringAsync(url);
        return ParseCity(jsonString, suggestion);
    }

    /// <summary> Fetches the weather data for a given <see cref="cityName"/> </summary>
    /// <param name="suggestion">the given city we want to find the weather data for</param>
    /// <returns>A <see cref="City"/> with current, hourly and daily weather.</returns>
    public async Task<City> FetchCityDataAsync(string cityName)
    {
        var suggestions = await GetCitySuggestionsAsync(cityName);
        if (suggestions.Count == 0)
        {
            throw new ArgumentException($"City '{cityName}' could not be found.");
        }

        return await FetchCityDataAsync(suggestions[0]);
    }

    /// <summary>
    /// This builds the url required for our GET request
    /// </summary>
    /// <param name="suggestion">This is the city that we're building the URL for</param>
    /// <returns>The url now ready for the HttpClient to request with</returns>
    private static string ForecastUrlBuilder(CitySuggestion suggestion)
    {
        string lat = suggestion.Latitude.ToString(CultureInfo.InvariantCulture);
        string lon = suggestion.Longitude.ToString(CultureInfo.InvariantCulture);
        // TODO: maybe the url should be less hardcoded ift daily, hourly osv?
        string url = $"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&daily=weather_code,temperature_2m_max,temperature_2m_min&hourly=temperature_2m,weather_code,precipitation_probability&current=temperature_2m,weather_code&timezone=auto";
        return url;
    }

    /// <summary>
    /// Parses an Open-Meteo forecast response into a <see cref="City"/>.
    /// </summary>
    /// <param name="jsonString">Raw JSON returned by the Open-Meteo forecast endpoint</param>
    /// <param name="suggestion">The city the user picked; supplies name, country and coordinates</param>
    /// <returns>The parsed city, with all the json now converted to a <see cref="City"/> </returns>
    /// <exception cref="JsonException">Thrown if <paramref name="jsonString"/> is not valid JSON</exception>
    /// <remarks>
    /// If the hourly or daily section can't be read, that forecast is left empty instead of failing.
    /// </remarks>
    private static City ParseCity(string jsonString, CitySuggestion suggestion) {
        
        // City består af de andre elementer i model, så vi starter fra bunden og arbejder mod toppen(city)
        using JsonDocument doc = JsonDocument.Parse(jsonString);
        JsonElement root = doc.RootElement;

       //Hour - HourlyForecast
       HourlyForecast hourlyForecast;
       try{
       JsonElement hourlyElement = root.GetProperty("hourly");
       List<DateTime> hourlyTimes = JsonSerializer.Deserialize<List<DateTime>>(hourlyElement.GetProperty("time").GetRawText()) ?? throw new JsonException("Time_hour is null");
       List<int> hourlyWeatherCodes = JsonSerializer.Deserialize<List<int>>(hourlyElement.GetProperty("weather_code").GetRawText()) ?? throw new JsonException("Weathercode_hour is null");
       List<double> hourlyTemps = JsonSerializer.Deserialize<List<double>>(hourlyElement.GetProperty("temperature_2m").GetRawText()) ?? throw new JsonException("Temperature_hour is null");
       List<int> precipitations = JsonSerializer.Deserialize<List<int>>(hourlyElement.GetProperty("precipitation_probability").GetRawText()) ?? throw new JsonException("Precipation_hour is null");

       List<Hour> hourList = [];
       for(int i = 0; i < hourlyTimes.Count; i++)
        {
            int hourOfDay = hourlyTimes[i].Hour;
            
            var hour = new Hour(
                time: hourOfDay,
                weatherCode: hourlyWeatherCodes[i],
                temperature: hourlyTemps[i],
                percipitation: precipitations[i]
            );

            hourList.Add(hour);
        }
        hourlyForecast = new HourlyForecast(hourList);
        }
        catch (JsonException)
        {
            hourlyForecast = new HourlyForecast([]);
        }

        // Day - DailyForecast
        DailyForecast dailyForecast;
        try{
        JsonElement dailyElement = root.GetProperty("daily");
        List<string> dates = JsonSerializer.Deserialize<List<string>>(dailyElement.GetProperty("time").GetRawText()) ?? throw new JsonException("Time_day is null");
        List<int> dailyWeatherCodes = JsonSerializer.Deserialize<List<int>>(dailyElement.GetProperty("weather_code").GetRawText()) ?? throw new JsonException("Weathercode_day is null");
        List<double> dailyMaxTemps = JsonSerializer.Deserialize<List<double>>(dailyElement.GetProperty("temperature_2m_max").GetRawText()) ?? throw new JsonException("Temperature_max is null");
        List<double> dailyMinTemps = JsonSerializer.Deserialize<List<double>>(dailyElement.GetProperty("temperature_2m_min").GetRawText()) ?? throw new JsonException("Temperature_min is null");

        List<Day> dayList = [];

        // Opretter objekterne en ad gangen med for loop
        for (int i = 0; i < dates.Count; i++){

        var day = new Day(
            date: dates[i],
            weatherCode: dailyWeatherCodes[i],
            tempMax: dailyMaxTemps[i],
            tempMin: dailyMinTemps[i]
        );

        dayList.Add(day);
        }
        dailyForecast = new DailyForecast(dayList);
        }
        catch (JsonException)
        {
            dailyForecast = new DailyForecast([]);
        }


        //CurrentWeather
        CurrentWeather currentWeather;
        try{
        JsonElement currentElement = root.GetProperty("current");
        var currentTemp = currentElement.GetProperty("temperature_2m").GetDouble();
        var currentWeatherCode = currentElement.GetProperty("weather_code").GetInt32();

        currentWeather = new CurrentWeather(
            temperature: currentTemp,
            weatherCode: currentWeatherCode, 
            0,
            new Wind(0, "N")
        );
        }
        catch
        {
            currentWeather = new CurrentWeather(0, 0, 0, new Wind(-1, "X"));
        }

        // Instantiate city
        var city = new City(
            cityName: suggestion.Name,
            country: suggestion.Country,
            longitude: suggestion.Longitude,
            latitude: suggestion.Latitude,
            currentWeather: currentWeather,
            hourlyForecast: hourlyForecast,
            dailyForecast: dailyForecast
            );

        if (!string.IsNullOrEmpty(suggestion.StateOrRegion))
        {
            city.StateOrRegion = suggestion.StateOrRegion;
        }

        return city;
        
    }

    public async Task<List<CitySuggestion>> GetCitySuggestionsAsync(string query)
    {
        string urlForRecommendations =
        $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(query)}&count=5&language=en&format=json";

        Console.WriteLine(urlForRecommendations);

        string jsonString = await _httpClient.GetStringAsync(urlForRecommendations);

        JsonDocument doc = JsonDocument.Parse(jsonString);

        if(!doc.RootElement.TryGetProperty("results", out JsonElement results))
        {
            return[];
        }

        List<CitySuggestion> suggestions = [];

        foreach (JsonElement result in results.EnumerateArray())
        {
            string stateOrRegion = "";
            if (result.TryGetProperty("admin1", out JsonElement admin1) && admin1.ValueKind == JsonValueKind.String)
            {
                stateOrRegion = admin1.GetString() ?? "";
            }
            suggestions.Add(new CitySuggestion
            {
                Name = result.GetProperty("name").GetString() ?? "Could not fetch city name",
                Country = result.GetProperty("country").GetString() ?? "Could not fetch city country",
                StateOrRegion = stateOrRegion,
                Latitude = result.GetProperty("latitude").GetDouble(),
                Longitude = result.GetProperty("longitude").GetDouble()
            });
        }
        return suggestions;
    }
}

