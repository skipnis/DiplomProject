using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Wishlists;

namespace Wishapp.Web.Users.Features.GetUserFulfilledWishes;

public record GetUserFulfilledWishesQuery(Guid TargetUserId) : IQuery<List<PublicFulfilledWishDto>>;
