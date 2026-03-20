using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Events.Features.LinkWishlist;

public record LinkWishlistCommand(Guid EventId, Guid UserId, Guid? WishlistId) : ICommand;
