using System.Text.Json.Serialization;

namespace Weather_WebAPI.Services;

internal sealed record GeocodingResponse(
    [property: JsonPropertyName("results")] List<GeocodingResult>? Results);

internal sealed record GeocodingResult(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("country")] string? Country,
    [property: JsonPropertyName("admin1")] string? Region,
    [property: JsonPropertyName("latitude")] double Latitude,
    [property: JsonPropertyName("longitude")] double Longitude,
    [property: JsonPropertyName("timezone")] string? Timezone);

internal sealed record ForecastResponse(
    [property: JsonPropertyName("latitude")] double Latitude,
    [property: JsonPropertyName("longitude")] double Longitude,
    [property: JsonPropertyName("timezone")] string Timezone,
    [property: JsonPropertyName("current")] ForecastCurrent Current,
    [property: JsonPropertyName("daily")] ForecastDaily Daily);

internal sealed record ForecastCurrent(
    [property: JsonPropertyName("time")] string Time,
    [property: JsonPropertyName("temperature_2m")] double Temperature,
    [property: JsonPropertyName("apparent_temperature")] double ApparentTemperature,
    [property: JsonPropertyName("relative_humidity_2m")] int Humidity,
    [property: JsonPropertyName("wind_speed_10m")] double WindSpeed,
    [property: JsonPropertyName("weather_code")] int WeatherCode,
    [property: JsonPropertyName("is_day")] int IsDay);

internal sealed record ForecastDaily(
    [property: JsonPropertyName("time")] List<string> Time,
    [property: JsonPropertyName("weather_code")] List<int?> WeatherCode,
    [property: JsonPropertyName("temperature_2m_min")] List<double?> MinTemperature,
    [property: JsonPropertyName("temperature_2m_max")] List<double?> MaxTemperature,
    [property: JsonPropertyName("precipitation_probability_max")] List<int?> PrecipitationProbability);
