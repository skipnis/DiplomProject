using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Admin.Features.Items.Update;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;

namespace Wishapp.Web.Admin;

public static partial class AdminEndpoints
{
    private static async Task<Results<NoContent, NotFound<Error>>> UpdateItem(
        [FromRoute] Guid id,
        UpdateCatalogItemRequest request,
        ICommandHandler<UpdateCatalogItemCommand> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(
            new UpdateCatalogItemCommand(
                id,
                request.Name,
                request.Description,
                request.Price,
                request.Currency,
                request.ImagePath,
                request.Url,
                request.CategoryId,
                request.IsPublished), ct);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : TypedResults.NotFound(result.Error);
    }
}
