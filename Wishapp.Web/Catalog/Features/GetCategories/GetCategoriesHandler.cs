using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Catalog.Dtos;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Catalog.Features.GetCategories;

public sealed class GetCategoriesHandler(ApplicationDbContext db)
    : IQueryHandler<GetCategoriesQuery, List<CatalogCategoryDto>>
{
    public async Task<Result<List<CatalogCategoryDto>>> HandleAsync(
        GetCategoriesQuery query,
        CancellationToken ct = default)
    {
        var categories = await db.CatalogCategories
            .AsNoTracking()
            .OrderBy(c => c.Order)
            .Select(c => new CatalogCategoryDto(c.Id, c.Name, c.Order))
            .ToListAsync(ct);

        return categories;
    }
}
