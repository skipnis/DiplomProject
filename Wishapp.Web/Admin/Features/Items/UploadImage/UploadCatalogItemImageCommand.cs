using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Admin.Features.Items.UploadImage;

public record UploadCatalogItemImageCommand(
    Guid ItemId,
    IFormFile? File,
    string? ExternalImageUrl) : ICommand<UploadCatalogItemImageResponse>;
