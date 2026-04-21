using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Admin.Features.Occasions.Delete;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;

namespace Wishapp.Web.Admin;

public static partial class AdminEndpoints
{
    private static async Task<Results<NoContent, NotFound<Error>>> DeleteOccasion(
        [FromRoute] Guid id,
        ICommandHandler<DeleteOccasionCommand> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new DeleteOccasionCommand(id), ct);

        if (!result.IsSuccess)
        {
            return TypedResults.NotFound(result.Error);
        }

        return TypedResults.NoContent();
    }
}
