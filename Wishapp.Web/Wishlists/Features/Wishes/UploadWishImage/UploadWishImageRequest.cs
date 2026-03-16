namespace Wishapp.Web.Wishlists.Features.Wishes.UploadWishImage;

public class UploadWishImageRequest
{
    public IFormFile? File { get; set; }
    public string? ExternalImageUrl { get; set; }
}