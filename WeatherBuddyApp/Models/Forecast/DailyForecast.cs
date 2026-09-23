namespace WeatherBuddyApp.Models.Forecast;
using System.Collections.Generic;
using System.Text;

public class DailyForecast(List<Day> dailyForecasting)
{
    public List<Day> DailyForecasting {get; set;} = dailyForecasting;

    public override string ToString()
    {
        if (DailyForecasting.Count == 0)
        {
            return "Daily Forecast: No data available";
        }

        var sb = new StringBuilder();
        sb.AppendLine("=== DAILY FORECAST ===");

        foreach (Day d in DailyForecasting)
        {
            sb.AppendLine($"  {d}"); // Calls hour.ToString() automatically
        }

        return sb.ToString().TrimEnd();
    }
}