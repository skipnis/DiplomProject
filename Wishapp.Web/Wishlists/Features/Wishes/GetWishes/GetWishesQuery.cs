using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Wishlists.Dtos;

namespace Wishapp.Web.Wishlists.Features.Wishes.GetWishes;

public record GetWishesQuery(
    Guid WishlistId,
    PagedRequest Request,
    bool HideReservations = false,
    WishSortBy SortBy = WishSortBy.CreatedAt,
    SortDirection Direction = SortDirection.Desc)
    : IQuery<PagedResponse<WishSummaryDto>>;