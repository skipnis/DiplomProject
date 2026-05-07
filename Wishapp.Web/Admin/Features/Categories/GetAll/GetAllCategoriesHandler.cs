using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Catalog.Dtos;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Admin.Features.Categories.GetAll;

public sealed class GetAllCategoriesHandler(ApplicationDbContext db)
    : IQueryHandler<GetAllCategoriesQuery, List<CatalogCategoryDto>>
{
    public async Task<Result<List<CatalogCategoryDto>>> HandleAsync(
        GetAllCategoriesQuery query,
        CancellationToken ct = default)
    {
        return await db.CatalogCategories
            .AsNoTracking()
            .OrderBy(c => c.Order)
            .Select(c => new CatalogCategoryDto(c.Id, c.Name, c.Order, c.IsPublished))
            .ToListAsync(ct);
    }
}
