namespace WeatherBuddyApp.Models.Forecast;

using System.Collections.Generic;
using System.Text;

public class HourlyForecast(List<Hour> hourlyForecasting)
{
    public List<Hour> HourlyForecasting {get; set;} = hourlyForecasting;

    public override string ToString()
    {
        if (HourlyForecasting.Count == 0)
        {
            return "Hourly Forecast: No data available";
        }

        var sb = new StringBuilder();
        sb.AppendLine("=== HOURLY FORECAST ===");

        for (int i = 0; i < 25 ; i++)
        {
            sb.AppendLine($"  {HourlyForecasting[i]}"); // Calls hour.ToString() automatically
        }

        return sb.ToString().TrimEnd();
    }
}