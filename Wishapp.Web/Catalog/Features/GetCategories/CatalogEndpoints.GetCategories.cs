using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Catalog.Dtos;
using Wishapp.Web.Catalog.Features.GetCategories;
using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Catalog;

public static partial class CatalogEndpoints
{
    private static async Task<Ok<List<CatalogCategoryDto>>> GetCategories(
        IQueryHandler<GetCategoriesQuery, List<CatalogCategoryDto>> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new GetCategoriesQuery(), ct);
        return TypedResults.Ok(result.Value);
    }
}
