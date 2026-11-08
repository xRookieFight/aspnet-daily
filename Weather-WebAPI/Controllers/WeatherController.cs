using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Weather_WebAPI.Services;

namespace Weather_WebAPI.Controllers;

[ApiController]
[Route("api/weather")]
public class WeatherController : ControllerBase
{
    private readonly WeatherService _weatherService;

    public WeatherController(WeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    [HttpGet]
    public async Task<IActionResult> GetByCity(
        [FromQuery, Required, StringLength(100, MinimumLength = 2)] string city,
        [FromQuery, Range(1, 16)] int days = 7,
        CancellationToken ct = default)
    {
        return await Guard(async () =>
        {
            var report = await _weatherService.GetByCityAsync(city, days, ct);
            if (report == null)
            {
                return NotFound(new ProblemDetails { Title = "City not found.", Status = StatusCodes.Status404NotFound });
            }
            return Ok(report);
        });
    }

    [HttpGet("coordinates")]
    public async Task<IActionResult> GetByCoordinates(
        [FromQuery, Required, Range(-90, 90)] double? latitude,
        [FromQuery, Required, Range(-180, 180)] double? longitude,
        [FromQuery, Range(1, 16)] int days = 7,
        CancellationToken ct = default)
    {
        return await Guard(async () =>
            Ok(await _weatherService.GetByCoordinatesAsync(latitude!.Value, longitude!.Value, days, ct)));
    }

    [HttpGet("locations")]
    public async Task<IActionResult> SearchLocations(
        [FromQuery, Required, StringLength(100, MinimumLength = 2)] string query,
        [FromQuery, Range(1, 10)] int count = 5,
        CancellationToken ct = default)
    {
        return await Guard(async () => Ok(await _weatherService.SearchLocationsAsync(query, count, ct)));
    }

    private async Task<IActionResult> Guard(Func<Task<IActionResult>> action)
    {
        try
        {
            return await action();
        }
        catch (WeatherProviderException ex)
        {
            return Problem(title: ex.Message, statusCode: StatusCodes.Status502BadGateway);
        }
    }
}
