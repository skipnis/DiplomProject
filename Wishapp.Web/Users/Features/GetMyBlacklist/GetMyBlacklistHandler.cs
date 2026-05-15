using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Users.Features.GetMyBlacklist;

public sealed class GetMyBlacklistHandler(ApplicationDbContext db)
    : IQueryHandler<GetMyBlacklistQuery, List<BlacklistItemResponse>>
{
    public async Task<Result<List<BlacklistItemResponse>>> HandleAsync(
        GetMyBlacklistQuery query,
        CancellationToken ct = default)
    {
        var items = await db.BlacklistItems
            .AsNoTracking()
            .Where(item => item.UserId == query.UserId)
            .OrderBy(item => item.CreatedAt)
            .Select(item => new BlacklistItemResponse(item.Id, item.Title, item.CreatedAt))
            .ToListAsync(ct);

        return items;
    }
}
