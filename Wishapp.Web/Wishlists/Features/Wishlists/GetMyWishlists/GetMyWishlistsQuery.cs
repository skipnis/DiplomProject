using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Wishlists.Dtos;

namespace Wishapp.Web.Wishlists.Features.Wishlists.GetMyWishlists;

public record GetMyWishlistsQuery(
    Guid UserId,
    PagedRequest Request,
    WishlistSortBy SortBy = WishlistSortBy.CreatedAt,
    SortDirection Direction = SortDirection.Desc)
    : IQuery<PagedResponse<WishlistSummaryDto>>;