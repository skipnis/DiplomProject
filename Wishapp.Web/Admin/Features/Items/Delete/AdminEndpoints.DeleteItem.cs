using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Admin.Features.Items.Delete;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;

namespace Wishapp.Web.Admin;

public static partial class AdminEndpoints
{
    private static async Task<Results<NoContent, NotFound<Error>>> DeleteItem(
        [FromRoute] Guid id,
        ICommandHandler<DeleteCatalogItemCommand> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new DeleteCatalogItemCommand(id), ct);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : TypedResults.NotFound(result.Error);
    }
}
