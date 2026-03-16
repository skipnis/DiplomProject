using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Wishlists.Features.Wishlists.CopyWish;

public record CopyWishCommand(
    Guid SourceWishlistId,
    Guid WishId,
    Guid TargetWishlistId,
    Guid UserId) : ICommand<CopyWishResponse>;