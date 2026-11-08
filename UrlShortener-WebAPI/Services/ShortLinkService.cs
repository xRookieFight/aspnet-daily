using System.Collections.Concurrent;
using System.Security.Cryptography;
using UrlShortener_WebAPI.Models;

namespace UrlShortener_WebAPI.Services;

public enum CreateLinkError
{
    None,
    InvalidUrl,
    AliasTaken
}

public class ShortLinkService
{
    private const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const int CodeLength = 7;

    private static readonly HashSet<string> ReservedCodes = new(StringComparer.OrdinalIgnoreCase) { "api", "js", "openapi" };

    private readonly ConcurrentDictionary<string, Entry> _links = new(StringComparer.Ordinal);

    public Task<IReadOnlyList<ShortLink>> GetAllLinksAsync()
    {
        IReadOnlyList<ShortLink> links = _links.Values
            .Select(e => e.ToModel())
            .OrderByDescending(l => l.CreatedAt)
            .ToList();
        return Task.FromResult(links);
    }

    public Task<ShortLink?> GetLinkAsync(string code)
    {
        return Task.FromResult(_links.TryGetValue(code, out var entry) ? entry.ToModel() : null);
    }

    public Task<(ShortLink? Link, CreateLinkError Error)> CreateLinkAsync(string url, string? alias)
    {
        if (!TryNormalizeUrl(url, out var targetUrl))
        {
            return Task.FromResult<(ShortLink?, CreateLinkError)>((null, CreateLinkError.InvalidUrl));
        }

        if (!string.IsNullOrEmpty(alias))
        {
            var entry = new Entry(alias, targetUrl, DateTime.UtcNow);
            if (ReservedCodes.Contains(alias) || !_links.TryAdd(alias, entry))
            {
                return Task.FromResult<(ShortLink?, CreateLinkError)>((null, CreateLinkError.AliasTaken));
            }

            return Task.FromResult<(ShortLink?, CreateLinkError)>((entry.ToModel(), CreateLinkError.None));
        }

        while (true)
        {
            var code = RandomNumberGenerator.GetString(Alphabet, CodeLength);
            var entry = new Entry(code, targetUrl, DateTime.UtcNow);
            if (_links.TryAdd(code, entry))
            {
                return Task.FromResult<(ShortLink?, CreateLinkError)>((entry.ToModel(), CreateLinkError.None));
            }
        }
    }

    public Task<string?> ResolveAsync(string code)
    {
        if (!_links.TryGetValue(code, out var entry))
        {
            return Task.FromResult<string?>(null);
        }

        entry.RegisterClick();
        return Task.FromResult<string?>(entry.TargetUrl);
    }

    public Task<bool> DeleteLinkAsync(string code)
    {
        return Task.FromResult(_links.TryRemove(code, out _));
    }

    private static bool TryNormalizeUrl(string url, out string normalized)
    {
        normalized = string.Empty;

        if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            || string.IsNullOrEmpty(uri.Host))
        {
            return false;
        }

        normalized = uri.AbsoluteUri;
        return true;
    }

    private sealed class Entry(string code, string targetUrl, DateTime createdAt)
    {
        private int _clicks;

        public string Code { get; } = code;
        public string TargetUrl { get; } = targetUrl;
        public DateTime CreatedAt { get; } = createdAt;

        public void RegisterClick() => Interlocked.Increment(ref _clicks);

        public ShortLink ToModel() => new()
        {
            Code = Code,
            TargetUrl = TargetUrl,
            Clicks = Volatile.Read(ref _clicks),
            CreatedAt = CreatedAt
        };
    }
}
