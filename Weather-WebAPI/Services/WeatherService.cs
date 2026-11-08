using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Weather_WebAPI.Models;

namespace Weather_WebAPI.Services;

public class WeatherProviderException(string message, Exception? inner = null) : Exception(message, inner);

public class WeatherService
{
    private const string CurrentFields = "temperature_2m,apparent_temperature,relative_humidity_2m,wind_speed_10m,weather_code,is_day";
    private const string DailyFields = "weather_code,temperature_2m_min,temperature_2m_max,precipitation_probability_max";

    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;
    private readonly OpenMeteoOptions _options;
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(HttpClient http, IMemoryCache cache, IOptions<OpenMeteoOptions> options, ILogger<WeatherService> logger)
    {
        _http = http;
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Location>> SearchLocationsAsync(string query, int count, CancellationToken ct)
    {
        var key = $"geo:{query.Trim().ToLowerInvariant()}:{count}";
        var locations = await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;

            var url = QueryHelpers.AddQueryString(_options.GeocodingUrl, new Dictionary<string, string?>
            {
                ["name"] = query.Trim(),
                ["count"] = count.ToString(CultureInfo.InvariantCulture),
                ["language"] = _options.Language,
                ["format"] = "json"
            });

            var response = await FetchAsync<GeocodingResponse>(url, ct);

            return (IReadOnlyList<Location>)(response.Results ?? [])
                .Select(r => new Location(r.Name, r.Region, r.Country, r.Latitude, r.Longitude, r.Timezone))
                .ToList();
        });

        return locations!;
    }

    public async Task<WeatherReport?> GetByCityAsync(string city, int days, CancellationToken ct)
    {
        var matches = await SearchLocationsAsync(city, 1, ct);
        if (matches.Count == 0)
        {
            return null;
        }

        var place = matches[0];
        var report = await GetByCoordinatesAsync(place.Latitude, place.Longitude, days, ct);
        return report with { Location = place with { Timezone = report.Location.Timezone } };
    }

    public async Task<WeatherReport> GetByCoordinatesAsync(double latitude, double longitude, int days, CancellationToken ct)
    {
        var lat = Math.Round(latitude, 2);
        var lon = Math.Round(longitude, 2);
        var key = string.Create(CultureInfo.InvariantCulture, $"wx:{lat}:{lon}:{days}");

        var report = await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;

            var url = QueryHelpers.AddQueryString(_options.ForecastUrl, new Dictionary<string, string?>
            {
                ["latitude"] = lat.ToString(CultureInfo.InvariantCulture),
                ["longitude"] = lon.ToString(CultureInfo.InvariantCulture),
                ["current"] = CurrentFields,
                ["daily"] = DailyFields,
                ["timezone"] = "auto",
                ["forecast_days"] = days.ToString(CultureInfo.InvariantCulture)
            });

            var response = await FetchAsync<ForecastResponse>(url, ct);
            return ToReport(response);
        });

        return report!;
    }

    private TimeSpan CacheDuration => TimeSpan.FromMinutes(Math.Max(_options.CacheMinutes, 0));

    private async Task<T> FetchAsync<T>(string url, CancellationToken ct)
    {
        try
        {
            var result = await _http.GetFromJsonAsync<T>(url, ct);
            return result ?? throw new WeatherProviderException("Weather provider returned an empty response.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Open-Meteo request failed with status {Status}", ex.StatusCode);
            throw new WeatherProviderException("Weather provider is unavailable.", ex);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "Open-Meteo request timed out");
            throw new WeatherProviderException("Weather provider timed out.", ex);
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            _logger.LogWarning(ex, "Open-Meteo returned an unexpected payload");
            throw new WeatherProviderException("Weather provider returned an unexpected response.", ex);
        }
    }

    private static WeatherReport ToReport(ForecastResponse response)
    {
        var current = response.Current;
        var currentWeather = new CurrentWeather(
            DateTime.Parse(current.Time, CultureInfo.InvariantCulture),
            current.Temperature,
            current.ApparentTemperature,
            current.Humidity,
            current.WindSpeed,
            current.WeatherCode,
            WeatherCodes.Describe(current.WeatherCode),
            current.IsDay == 1);

        var daily = response.Daily;
        var forecast = daily.Time
            .Select((date, i) =>
            {
                var code = daily.WeatherCode.ElementAtOrDefault(i);
                return new DailyForecast(
                    DateOnly.Parse(date, CultureInfo.InvariantCulture),
                    daily.MinTemperature.ElementAtOrDefault(i),
                    daily.MaxTemperature.ElementAtOrDefault(i),
                    daily.PrecipitationProbability.ElementAtOrDefault(i),
                    code,
                    code is { } c ? WeatherCodes.Describe(c) : null);
            })
            .ToList();

        var location = new Location(null, null, null, response.Latitude, response.Longitude, response.Timezone);
        return new WeatherReport(location, currentWeather, forecast);
    }
}
