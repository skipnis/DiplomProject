using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Wishlists.Features.Wishes.UploadWishImage;

public record UploadWishImageCommand(
    Guid WishlistId,
    Guid WishId,
    IFormFile? File,
    string? ExternalImageUrl) : ICommand<UploadWishImageResponse>;