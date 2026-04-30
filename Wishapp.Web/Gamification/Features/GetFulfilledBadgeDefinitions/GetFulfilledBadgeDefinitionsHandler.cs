using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Gamification.Features.GetFulfilledBadgeDefinitions;

public record FulfilledBadgeDefinitionDto(int Id, string Emoji, string Slug, string Label, string Description, bool IsActive);

public record GetFulfilledBadgeDefinitionsQuery : IQuery<List<FulfilledBadgeDefinitionDto>>;

public sealed class GetFulfilledBadgeDefinitionsHandler(ApplicationDbContext db)
    : IQueryHandler<GetFulfilledBadgeDefinitionsQuery, List<FulfilledBadgeDefinitionDto>>
{
    public async Task<Result<List<FulfilledBadgeDefinitionDto>>> HandleAsync(
        GetFulfilledBadgeDefinitionsQuery query,
        CancellationToken ct = default)
    {
        return await db.FulfilledWishBadgeDefinitions
            .AsNoTracking()
            .OrderBy(b => b.Id)
            .Select(b => new FulfilledBadgeDefinitionDto(b.Id, b.Emoji, b.Slug, b.Label, b.Description, b.IsActive))
            .ToListAsync(ct);
    }
}
