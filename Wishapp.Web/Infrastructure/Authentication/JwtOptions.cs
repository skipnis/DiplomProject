using System.ComponentModel.DataAnnotations;

namespace Wishapp.Web.Infrastructure.Authentication;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    
    [MinLength(32)]
    public required string Secret { get; set; } 
    [MinLength(1)]
    public required string Issuer { get; set; } 
    [MinLength(1)]
    public required string Audience { get; set; }
    [Required]
    public int ExpirationInMinutes { get; set; }
    [Required]
    public int RefreshTokenExpirationInDays { get; set; }
}
