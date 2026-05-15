using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Wishlists.Dtos;

namespace Wishapp.Web.Wishlists.Features.Wishlists.GetUserWishlists;

public record GetUserWishlistsQuery(
    Guid? CurrentUserId,
    Guid TargetUserId,
    PagedRequest Request,
    WishlistSortBy SortBy = WishlistSortBy.CreatedAt,
    SortDirection Direction = SortDirection.Desc) : IQuery<PagedResponse<WishlistSummaryDto>>;