using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Admin.Features.Categories.GetAll;
using Wishapp.Web.Catalog.Dtos;
using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Admin;

public static partial class AdminEndpoints
{
    private static async Task<Ok<List<CatalogCategoryDto>>> GetAllCategories(
        IQueryHandler<GetAllCategoriesQuery, List<CatalogCategoryDto>> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new GetAllCategoriesQuery(), ct);
        return TypedResults.Ok(result.Value);
    }
}
