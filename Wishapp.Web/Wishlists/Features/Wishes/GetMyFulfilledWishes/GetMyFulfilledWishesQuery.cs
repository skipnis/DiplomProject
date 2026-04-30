using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Wishlists.Features.Wishes.GetMyFulfilledWishes;

public sealed record GetMyFulfilledWishesQuery(Guid UserId) : IQuery<List<FulfilledWishRecordDto>>;
