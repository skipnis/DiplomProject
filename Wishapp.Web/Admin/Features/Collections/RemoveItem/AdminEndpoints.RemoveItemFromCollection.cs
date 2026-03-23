using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Admin.Features.Collections.RemoveItem;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;

namespace Wishapp.Web.Admin;

public static partial class AdminEndpoints
{
    private static async Task<Results<NoContent, NotFound<Error>>> RemoveItemFromCollection(
        [FromRoute] Guid id,
        [FromRoute] Guid itemId,
        ICommandHandler<RemoveItemFromCollectionCommand> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new RemoveItemFromCollectionCommand(id, itemId), ct);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : TypedResults.NotFound(result.Error);
    }
}
