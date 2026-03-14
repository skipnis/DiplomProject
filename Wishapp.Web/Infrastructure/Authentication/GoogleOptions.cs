using System.ComponentModel.DataAnnotations;

namespace Wishapp.Web.Infrastructure.Authentication;

public class GoogleOptions
{
    public const string SectionName = "Google";
    [Required]
    public required string ClientId { get; set; }
}