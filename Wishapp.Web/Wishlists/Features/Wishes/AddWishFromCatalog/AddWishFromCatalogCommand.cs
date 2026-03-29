using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Wishlists.Features.Wishes.AddWishFromCatalog;

public record AddWishFromCatalogCommand(
    Guid WishlistId,
    Guid CatalogItemId,
    Guid UserId) : ICommand<Guid>;
