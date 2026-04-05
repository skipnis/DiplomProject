using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Catalog.Dtos;
using Wishapp.Web.Catalog.Features.GetCatalogItems;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Extensions;

namespace Wishapp.Web.Catalog;

public static partial class CatalogEndpoints
{
    private static async Task<Ok<PagedResponse<CatalogItemDto>>> GetCatalogItems(
        [AsParameters] CatalogItemsRequest request,
        ClaimsPrincipal user,
        IQueryHandler<GetCatalogItemsQuery, PagedResponse<CatalogItemDto>> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();
        var userId = userIdResult.IsSuccess ? (Guid?)userIdResult.Value : null;

        var result = await handler.HandleAsync(
            new GetCatalogItemsQuery(request.ToFilter(), request.ToPaged(), userId), ct);
        return TypedResults.Ok(result.Value);
    }
}
