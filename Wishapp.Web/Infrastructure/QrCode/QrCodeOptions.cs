using System.ComponentModel.DataAnnotations;

namespace Wishapp.Web.Infrastructure.QrCode;

public sealed class QrCodeOptions
{
    public const string SectionName = "QrCode";

    [Required]
    [MinLength(1)]
    public string FrontendUrl { get; set; } = default!;
}