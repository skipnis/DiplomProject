using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Gamification.Features.GetCatalogBadgeDefinitions;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Admin.Features.CatalogBadgeDefinitions.GetAll;

public sealed class GetAllCatalogBadgeDefinitionsHandler(ApplicationDbContext db)
    : IQueryHandler<GetAllCatalogBadgeDefinitionsQuery, List<CatalogBadgeDefinitionDto>>
{
    public async Task<Result<List<CatalogBadgeDefinitionDto>>> HandleAsync(
        GetAllCatalogBadgeDefinitionsQuery query,
        CancellationToken ct = default)
    {
        var items = await db.CatalogBadgeDefinitions
            .AsNoTracking()
            .OrderBy(b => b.Id)
            .Select(b => new CatalogBadgeDefinitionDto(b.Id, b.Emoji, b.Slug, b.Label, b.Description, b.IsActive))
            .ToListAsync(ct);

        return items;
    }
}
