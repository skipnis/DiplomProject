using System.ComponentModel.DataAnnotations;

namespace Wishapp.Web.Infrastructure.ObjectStorage;

public sealed class ObjectStorageOptions
{
    public const string SectionName = "ObjectStorage";

    [Required]
    [MinLength(1)]
    public required string Endpoint { get; set; }
    [Required]
    [MinLength(1)]
    public required string AccessKey { get; set; }
    [Required]
    [MinLength(1)]
    public required string SecretKey { get; set; }
    [Required]
    [MinLength(1)]
    public required string BucketName { get; set; }
    [Required]
    [MinLength(1)]
    public required string PublicUrl { get; set; }

    public bool UseSSL { get; set; } = false;
}
