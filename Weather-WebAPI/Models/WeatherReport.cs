namespace Weather_WebAPI.Models;

public record WeatherReport(Location Location, CurrentWeather Current, IReadOnlyList<DailyForecast> Daily);

public record Location(string? Name, string? Region, string? Country, double Latitude, double Longitude, string? Timezone);

public record CurrentWeather(
    DateTime Time,
    double Temperature,
    double ApparentTemperature,
    int Humidity,
    double WindSpeed,
    int WeatherCode,
    string Description,
    bool IsDay);

public record DailyForecast(
    DateOnly Date,
    double? MinTemperature,
    double? MaxTemperature,
    int? PrecipitationProbability,
    int? WeatherCode,
    string? Description);
