using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Wishlists.Dtos;

namespace Wishapp.Web.Wishlists.Features.Wishes.ParseWithUrl;

public record ParseWishUrlQuery(string Url) : IQuery<ParsedWishData>;