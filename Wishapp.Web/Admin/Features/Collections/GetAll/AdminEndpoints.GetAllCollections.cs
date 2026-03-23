using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Admin.Features.Collections.GetAll;
using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Admin;

public static partial class AdminEndpoints
{
    private static async Task<Ok<List<CatalogCollectionAdminDto>>> GetAllCollections(
        IQueryHandler<GetAllCollectionsQuery, List<CatalogCollectionAdminDto>> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new GetAllCollectionsQuery(), ct);
        return TypedResults.Ok(result.Value);
    }
}
