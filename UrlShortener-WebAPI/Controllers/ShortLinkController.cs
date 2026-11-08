using Microsoft.AspNetCore.Mvc;
using UrlShortener_WebAPI.Models;
using UrlShortener_WebAPI.Services;

namespace UrlShortener_WebAPI.Controllers;

[ApiController]
[Route("api/links")]
public class ShortLinkController : ControllerBase
{
    private const string CodeConstraint = "regex(^[[A-Za-z0-9_-]]{{3,32}}$)";

    private readonly ShortLinkService _linkService;

    public ShortLinkController(ShortLinkService linkService)
    {
        _linkService = linkService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllLinks()
    {
        var links = await _linkService.GetAllLinksAsync();
        return Ok(links.Select(ToResponse));
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> GetLink(string code)
    {
        var link = await _linkService.GetLinkAsync(code);
        if (link == null)
        {
            return NotFound();
        }
        return Ok(ToResponse(link));
    }

    [HttpPost]
    public async Task<IActionResult> CreateLink([FromBody] CreateShortLinkRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (link, error) = await _linkService.CreateLinkAsync(request.Url, request.Alias);
        switch (error)
        {
            case CreateLinkError.InvalidUrl:
                ModelState.AddModelError(nameof(request.Url), "Url must be an absolute http or https address.");
                return ValidationProblem(ModelState);
            case CreateLinkError.AliasTaken:
                return Conflict(new ProblemDetails { Title = "Alias is already taken.", Status = StatusCodes.Status409Conflict });
        }

        return CreatedAtAction(nameof(GetLink), new { code = link!.Code }, ToResponse(link));
    }

    [HttpDelete("{code}")]
    public async Task<IActionResult> DeleteLink(string code)
    {
        var deleted = await _linkService.DeleteLinkAsync(code);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpGet("/{code:" + CodeConstraint + "}")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> Follow(string code)
    {
        var target = await _linkService.ResolveAsync(code);
        if (target == null)
        {
            return NotFound();
        }

        return Redirect(target);
    }

    private ShortLinkResponse ToResponse(ShortLink link) =>
        new(link.Code, $"{Request.Scheme}://{Request.Host}/{link.Code}", link.TargetUrl, link.Clicks, link.CreatedAt);
}
