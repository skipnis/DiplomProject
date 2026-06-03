using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Events.Dtos;

namespace Wishapp.Web.Events.Features.GetEventByWishlist;

public record GetEventByWishlistQuery(Guid WishlistId, Guid UserId) : IQuery<EventDto>;
