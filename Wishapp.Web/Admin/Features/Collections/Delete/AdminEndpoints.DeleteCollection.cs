using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Admin.Features.Collections.Delete;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;

namespace Wishapp.Web.Admin;

public static partial class AdminEndpoints
{
    private static async Task<Results<NoContent, NotFound<Error>>> DeleteCollection(
        [FromRoute] Guid id,
        ICommandHandler<DeleteCollectionCommand> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new DeleteCollectionCommand(id), ct);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : TypedResults.NotFound(result.Error);
    }
}
