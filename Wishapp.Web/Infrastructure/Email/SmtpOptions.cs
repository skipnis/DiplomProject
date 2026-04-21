using System.ComponentModel.DataAnnotations;

namespace Wishapp.Web.Infrastructure.Email;

public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";

    [Required]
    public string Host { get; init; } = string.Empty;

    [Required]
    public int Port { get; init; }

    [Required]
    public string From { get; init; } = string.Empty;

    public string? Username { get; init; }
    public string? Password { get; init; }
}
