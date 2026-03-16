using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Wishlists.Dtos;

namespace Wishapp.Web.Wishlists.Features.Wishlists.GetMyWishlists;

public record GetMyWishlistsQuery(Guid UserId, PagedRequest Request)
    : IQuery<PagedResponse<WishlistSummaryDto>>;