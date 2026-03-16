using System.ComponentModel.DataAnnotations;

namespace Wishapp.Web.Infrastructure.Minio;

public sealed class MinioOptions
{
    public const string SectionName = "Minio";

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
}