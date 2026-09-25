

namespace WeatherBuddyApp.Models;
public class CitySuggestion
{
    public string Name { get; set; } = "";
    public string Country { get; set; } = "";
    public string StateOrRegion {get; set;} = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}