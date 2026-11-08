namespace UrlShortener_WebAPI.Models;

public record ShortLinkResponse(string Code, string ShortUrl, string TargetUrl, int Clicks, DateTime CreatedAt);
