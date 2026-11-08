namespace UrlShortener_WebAPI.Models;

public class ShortLink
{
    public string Code { get; set; } = string.Empty;

    public string TargetUrl { get; set; } = string.Empty;

    public int Clicks { get; set; }

    public DateTime CreatedAt { get; set; }
}
