using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Gamification.Features.GetCatalogBadgeDefinitions;

public record CatalogBadgeDefinitionDto(int Id, string Emoji, string Slug, string Label, string Description, bool IsActive);

public record GetCatalogBadgeDefinitionsQuery : IQuery<List<CatalogBadgeDefinitionDto>>;

public sealed class GetCatalogBadgeDefinitionsHandler(ApplicationDbContext db)
    : IQueryHandler<GetCatalogBadgeDefinitionsQuery, List<CatalogBadgeDefinitionDto>>
{
    public async Task<Result<List<CatalogBadgeDefinitionDto>>> HandleAsync(
        GetCatalogBadgeDefinitionsQuery query,
        CancellationToken ct = default)
    {
        return await db.CatalogBadgeDefinitions
            .AsNoTracking()
            .OrderBy(b => b.Id)
            .Select(b => new CatalogBadgeDefinitionDto(b.Id, b.Emoji, b.Slug, b.Label, b.Description, b.IsActive))
            .ToListAsync(ct);
    }
}
