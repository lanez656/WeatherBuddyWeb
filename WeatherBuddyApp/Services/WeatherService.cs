using System.Globalization;
using System.Text.Json;
using WeatherBuddyApp.Models;
using WeatherBuddyApp.Models.Forecast;

namespace WeatherBuddyApp.Services;


public class WeatherService(HttpClient httpClient) : IWeatherService
{

    private readonly HttpClient _httpClient = httpClient;
    
    /// <summary>Fetches the weather data for a <see cref="CitySuggestion"/></summary>
    /// <param name="suggestion">the city</param>
    /// <returns>the <see cref="CitySuggestion"/> transformed to the <see cref="City"/> with all the weather data we need</returns>
    public async Task<City> FetchCityDataAsync(CitySuggestion suggestion)
    {
        string url = ForecastUrlBuilder(suggestion);
        string jsonString = await _httpClient.GetStringAsync(url);
        return ParseCity(jsonString, suggestion);
    }
    
    /// <summary>Looks up a city by name and fetches the weather for the first match.</summary>
    /// <param name="cityName">The name the user typed, f.eks. "Esbjerg"</param>
    /// <returns>A <see cref="City"/> with current, hourly and daily weather.</returns>
    /// <exception cref="ArgumentException">Thrown if no city matches the name.</exception>
    public async Task<City> FetchCityDataAsync(string cityName)
    {
        var suggestions = await GetCitySuggestionsAsync(cityName);
        if (suggestions.Count == 0)
        {
            throw new ArgumentException($"City '{cityName}' could not be found.");
        }

        return await FetchCityDataAsync(suggestions[0]);
    }
    
    /// <summary>asks Open-Meteo’s geocoding API for cities matching the text in search field and returns a list of CitySuggestion(s)</summary>
    /// <param name="query">the query from the search.js</param>
    /// <returns>A List of the CitySuggestion(s)</returns>
    public async Task<List<CitySuggestion>> GetCitySuggestionsAsync(string query)
    {
        string urlForRecommendations =
        $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(query)}&count=5&language=en&format=json";

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
    
    /// <summary>This builds the url required for our GET request</summary>
    /// <param name="suggestion">This is the city that we're building the URL for</param>
    /// <returns>The url now ready for the HttpClient to request with</returns>
    private static string ForecastUrlBuilder(CitySuggestion suggestion)
    {
        string lat = suggestion.Latitude.ToString(CultureInfo.InvariantCulture);
        string lon = suggestion.Longitude.ToString(CultureInfo.InvariantCulture);
        // TODO: maybe the url should be less hardcoded ift daily, hourly osv?
        string url = $"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&daily=weather_code,temperature_2m_max,temperature_2m_min,sunset,sunrise&hourly=temperature_2m,weather_code,precipitation_probability&current=temperature_2m,apparent_temperature,relative_humidity_2m,wind_speed_10m,wind_direction_10m,weather_code&timezone=auto";
        return url;
    }

    /// <summary>Parses the "current" JsonElement from the full api call to get the forecast data for right now into our <see cref="CurrentWeather"/> </summary>
    /// <param name="root">the root element from the json</param>
    /// <returns>our <see cref="CurrentWeather"/></returns>
    private static CurrentWeather ParseCurrentWeather(JsonElement root)
    {
        try
        {
            JsonElement currentElement = root.GetProperty("current");
            double currentTemp = currentElement.GetProperty("temperature_2m").GetDouble();
            double currentApparentTemperature = currentElement.GetProperty("apparent_temperature").GetDouble();
            int currentWeatherCode = currentElement.GetProperty("weather_code").GetInt32();
            int currentHumidity = currentElement.GetProperty("relative_humidity_2m").GetInt32();
            double currentWindSpeed = currentElement.GetProperty("wind_speed_10m").GetDouble();
            double currentWindDirection = currentElement.GetProperty("wind_direction_10m").GetDouble();
            var currentTime = DateTime.ParseExact(currentElement.GetProperty("time").GetString()!,"yyyy-MM-dd'T'HH:mm",CultureInfo.InvariantCulture);

            return new CurrentWeather(
                temperature: currentTemp,
                apparentTemperature: currentApparentTemperature,
                weatherCode: currentWeatherCode,
                humidity: currentHumidity,
                new Wind(currentWindSpeed/3.6, ToWord(currentWindDirection)),
                currentTime: currentTime);
        }
        catch
        {
            throw new JsonException("Could not parse current weather from the response.");
        }
    }
    
    /// <summary>Parses the "hourly" JsonElement from the full api call to get the hourly forecast data into our <see cref="HourlyForecast"/> </summary>
    /// <param name="root">the root element from the json</param>
    /// <returns>our <see cref="HourlyForecast"/></returns>    
    private static HourlyForecast ParseHourlyForecast(JsonElement root)
    {
        try
        {
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
                    percipitation: precipitations[i]);
                hourList.Add(hour);
            }
            return new HourlyForecast(hourList);
        }
        catch 
        {
            return new HourlyForecast([]);
        }
    }
    
    /// <summary>Parses the "daily" JsonElement from the full api call to get the daily forecast data into our <see cref="DailyForecast"/> </summary>
    /// <param name="root">the root element from the json</param>
    /// <returns>our <see cref="DailyForecast"/></returns>
    private static DailyForecast ParseDailyForecast(JsonElement root)
    {
        try
        {
            JsonElement dailyElement = root.GetProperty("daily");
            List<string> dates = JsonSerializer.Deserialize<List<string>>(dailyElement.GetProperty("time").GetRawText()) ?? throw new JsonException("Time_day is null");
            List<int> dailyWeatherCodes = JsonSerializer.Deserialize<List<int>>(dailyElement.GetProperty("weather_code").GetRawText()) ?? throw new JsonException("Weathercode_day is null");
            List<double> dailyMaxTemps = JsonSerializer.Deserialize<List<double>>(dailyElement.GetProperty("temperature_2m_max").GetRawText()) ?? throw new JsonException("Temperature_max is null");
            List<double> dailyMinTemps = JsonSerializer.Deserialize<List<double>>(dailyElement.GetProperty("temperature_2m_min").GetRawText()) ?? throw new JsonException("Temperature_min is null");
            List<DateTime> dailySunrises = JsonSerializer.Deserialize<List<DateTime>>(dailyElement.GetProperty("sunrise").GetRawText()) ?? throw new JsonException("sunrise is null");
            List<DateTime> dailySunsets = JsonSerializer.Deserialize<List<DateTime>>(dailyElement.GetProperty("sunset").GetRawText()) ?? throw new JsonException("sunset is null");
            List<Day> dayList = [];

            for (int i = 0; i < dates.Count; i++)
            {
                var day = new Day(
                    date: dates[i],
                    weatherCode: dailyWeatherCodes[i],
                    tempMax: dailyMaxTemps[i],
                    tempMin: dailyMinTemps[i],
                    sunrise: dailySunrises[i],
                    sunset: dailySunsets[i]);
                dayList.Add(day);
            }
            return new DailyForecast(dayList);
        }
        catch
        {
            return new DailyForecast([]);
        }
    }

    /// <summary>Parses an Open-Meteo forecast response into a <see cref="City"/></summary>
    /// <param name="jsonString">Raw JSON returned by the Open-Meteo forecast endpoint</param>
    /// <param name="suggestion">The city the user picked; supplies name, country and coordinates</param>
    /// <returns>The parsed city, with all the json now converted to a <see cref="City"/> </returns>
    /// <exception cref="JsonException">Thrown if <paramref name="jsonString"/> is not valid JSON or if ParseCurrentWeather fails</exception>
    /// <remarks>If the hourly or daily section can't be read, that forecast is left empty instead of failing.</remarks>
    private static City ParseCity(string jsonString, CitySuggestion suggestion) 
    {
        
        // City består af de andre elementer i model, så vi starter fra bunden og arbejder mod toppen(city)
        using JsonDocument doc = JsonDocument.Parse(jsonString);
        JsonElement root = doc.RootElement;
        
        CurrentWeather currentWeather = ParseCurrentWeather(root);
        HourlyForecast hourlyForecast = ParseHourlyForecast(root);
        DailyForecast dailyForecast = ParseDailyForecast(root);

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

    /// <summary>little helper method to convert the degrees (out to 360) of which way the wind is blowing, to the direction</summary>
    /// <param name="degrees">The direction the wind is coming from, in degrees (0 = north, 90 = east, 180 = south, 270 = west)</param>
    /// <returns>The direction as the word</returns>
    private static string ToWord(double degrees)
    {
        string[] direction = ["North", "Northeast", "East", "Southeast", "South", "Southwest", "West", "Northwest"];
        return direction[(int)Math.Round(degrees / 45.0) % 8];
    }
}

