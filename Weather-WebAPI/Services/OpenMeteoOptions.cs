namespace Weather_WebAPI.Services;

public class OpenMeteoOptions
{
    public const string SectionName = "OpenMeteo";

    public string GeocodingUrl { get; set; } = "https://geocoding-api.open-meteo.com/v1/search";

    public string ForecastUrl { get; set; } = "https://api.open-meteo.com/v1/forecast";

    public string Language { get; set; } = "tr";

    public int CacheMinutes { get; set; } = 10;

    public int TimeoutSeconds { get; set; } = 10;
}
