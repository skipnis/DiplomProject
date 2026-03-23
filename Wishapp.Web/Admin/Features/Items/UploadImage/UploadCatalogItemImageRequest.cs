namespace Wishapp.Web.Admin.Features.Items.UploadImage;

public class UploadCatalogItemImageRequest
{
    public IFormFile? File { get; set; }
    public string? ExternalImageUrl { get; set; }
}
