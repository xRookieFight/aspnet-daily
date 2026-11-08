using System.ComponentModel.DataAnnotations;

namespace UrlShortener_WebAPI.Models;

public class CreateShortLinkRequest
{
    [Required]
    [StringLength(2048, MinimumLength = 1)]
    public string Url { get; set; } = string.Empty;

    [RegularExpression("^[A-Za-z0-9_-]{3,32}$", ErrorMessage = "Alias must be 3 to 32 letters, digits, '_' or '-'.")]
    public string? Alias { get; set; }
}
