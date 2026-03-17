using Wishapp.Web.Wishlists.Dtos;

namespace Wishapp.Web.Wishlists;

public interface IWishlistsApi
{
    Task CreateSystemWishlistsAsync(Guid userId, CancellationToken ct = default);
    Task<bool> WishExistsAsync(Guid wishId, CancellationToken ct = default);
    Task<bool> CanAccessWishlistAsync(Guid userId, Guid wishlistId, CancellationToken ct = default);
    Task<WishlistAccessData?> GetWishlistAccessDataAsync(Guid wishlistId, CancellationToken ct = default);
    Task<bool> IsWishFulfilledAsync(Guid wishId, CancellationToken ct = default);
    Task<List<WishSummary>> GetWishesSummaryAsync(List<Guid> wishIds, CancellationToken ct = default);
}
