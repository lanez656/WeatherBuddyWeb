

namespace WeatherBuddyApp.Services;

public static class WeatherCodeDictionary {

    public static string TranslateCode(int i)
    {
        return i switch
        {
            0 => "Clear Sky",
            1 => "Mainly Clear",
            2 => "Partly Cloudy",
            3 => "Overcast",
            45 => "Fog",
            48 => "Depositing Rime Fog",
            51 => "Light Drizzle",
            53 => "Moderate Drizzle",
            55 => "Dense Drizzle",
            56 => "Light Freeing Drizzle",
            57 => "Dense Freezing Drizzle",
            61 => "Light Rain",
            63 => "Moderate Rain",
            65 => "Heavy Rain",
            66 => "Light Freezing Rain",
            67 => "Heavy Freezing Rain",
            71 => "Light Snow",
            73 => "Moderate Snow",
            75 => "Heavy Snow",
            77 => "Snow Grains",
            80 => "Slight Rain Showers",
            81 => "Moderate Rain Showers",
            82 => "Violent Rain Showers",
            85 => "Light Snow Shower",
            86 => "Heavy Snow Shower",
            95 => "Thunderstorm",
            96 => "Thunderstorm With Hail",
            99 => "Thunderstorm With Hail",
            _ => "Could Not Translate Weathercode",
        };
    }
    
}