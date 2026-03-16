using Wishapp.Web.Wishlists.Dtos;

namespace Wishapp.Web.Infrastructure.Interfaces;

public interface IUrlParser
{
    Task<ParsedWishData> ParseAsync(string url, CancellationToken ct = default);
}