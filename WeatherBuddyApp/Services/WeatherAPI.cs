using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using System.Threading.Tasks;
using WeatherBuddyApp.Models;
using WeatherBuddyApp.Models.Forecast;

namespace WeatherBuddyApp.Services;


public static class WeatherApi
{
    private static readonly HttpClient client = new HttpClient();

    public static async Task<City> FetchCityDataAsync(string cityName)
    {
        var (url, country) = await CoordinatesUrlBuilder(cityName);

        string jsonString = await client.GetStringAsync(url);
        using JsonDocument doc = JsonDocument.Parse(jsonString);
        JsonElement root = doc.RootElement;

       // City består af de andre elementer i model, så vi starter fra bunden og arbejder mod toppen(city)

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

        List<Day> dayList = new List<Day>();

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
            cityName: cityName,
            country: country,
            currentWeather: currentWeather,
            hourlyForecast: hourlyForecast,
            dailyForecast: dailyForecast
            );

        return city;
        

    }


    // CoordinatesUrlBuilder henter koordinaterne fra OpenMateos geocoding api og indsætter dem i url'en for vores api til dataen.
    public static async Task<(string Url, string Country)> CoordinatesUrlBuilder (string cityName)
    {

        string urlForCoordinates = $"https://geocoding-api.open-meteo.com/v1/search?name={cityName}&count=1&language=en&format=json";

        string jsonString = await client.GetStringAsync(urlForCoordinates);
        using JsonDocument doc = JsonDocument.Parse(jsonString);
        JsonElement root = doc.RootElement;


        if (!root.TryGetProperty("results", out JsonElement resultsElement) || resultsElement.GetArrayLength() == 0)
        {
            throw new ArgumentException($"City '{cityName}' could not be found.");
        }

        JsonElement firstResult = root.GetProperty("results");
        JsonElement cityElement = firstResult[0];


        var latitude = cityElement.GetProperty("latitude").GetRawText();
        var longitude = cityElement.GetProperty("longitude").GetRawText();
        var country = cityElement.GetProperty("country").GetRawText().Trim('"');
        string coordinates = $"latitude={latitude}&longitude={longitude}";
        
        string urlForDataCall = $"https://api.open-meteo.com/v1/forecast?{coordinates}&daily=weather_code,temperature_2m_max,temperature_2m_min&hourly=temperature_2m,weather_code,precipitation_probability&current=temperature_2m,weather_code&timezone=Europe%2FBerlin";

        return (urlForDataCall, country);
    } 
}

