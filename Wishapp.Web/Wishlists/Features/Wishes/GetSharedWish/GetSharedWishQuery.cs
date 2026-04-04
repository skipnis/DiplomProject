using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Wishlists.Features.Wishes.GetSharedWish;

public record GetSharedWishQuery(Guid Token) : IQuery<SharedWishResponse>;
