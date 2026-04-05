using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Catalog.Dtos;
using Wishapp.Web.Catalog.Features.GetCatalogItem;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Extensions;

namespace Wishapp.Web.Catalog;

public static partial class CatalogEndpoints
{
    private static async Task<Results<Ok<CatalogItemDto>, NotFound<Error>>> GetCatalogItem(
        [FromRoute] Guid id,
        ClaimsPrincipal user,
        IQueryHandler<GetCatalogItemQuery, CatalogItemDto> handler,
        CancellationToken ct)
    {
        var userIdResult = user.TryGetUserId();
        var userId = userIdResult.IsSuccess ? (Guid?)userIdResult.Value : null;

        var result = await handler.HandleAsync(new GetCatalogItemQuery(id, userId), ct);

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.NotFound(result.Error);
    }
}
