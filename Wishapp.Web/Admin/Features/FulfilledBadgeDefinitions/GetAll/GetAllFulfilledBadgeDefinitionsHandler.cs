using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Gamification.Features.GetFulfilledBadgeDefinitions;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Admin.Features.FulfilledBadgeDefinitions.GetAll;

public sealed class GetAllFulfilledBadgeDefinitionsHandler(ApplicationDbContext db)
    : IQueryHandler<GetAllFulfilledBadgeDefinitionsQuery, List<FulfilledBadgeDefinitionDto>>
{
    public async Task<Result<List<FulfilledBadgeDefinitionDto>>> HandleAsync(
        GetAllFulfilledBadgeDefinitionsQuery query,
        CancellationToken ct = default)
    {
        var items = await db.FulfilledWishBadgeDefinitions
            .AsNoTracking()
            .OrderBy(b => b.Id)
            .Select(b => new FulfilledBadgeDefinitionDto(b.Id, b.Emoji, b.Slug, b.Label, b.Description, b.IsActive))
            .ToListAsync(ct);

        return items;
    }
}
