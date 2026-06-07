using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Admin.Features.Occasions.Update;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;

namespace Wishapp.Web.Admin;

public static partial class AdminEndpoints
{
    private static async Task<Results<NoContent, NotFound<Error>, Conflict<Error>>> UpdateOccasion(
        [FromRoute] Guid id,
        UpdateOccasionRequest request,
        ICommandHandler<UpdateOccasionCommand> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new UpdateOccasionCommand(id, request.Key, request.Label), ct);

        if (!result.IsSuccess)
        {
            return result.Error.Type == ErrorType.Conflict
                ? TypedResults.Conflict(result.Error)
                : TypedResults.NotFound(result.Error);
        }

        return TypedResults.NoContent();
    }
}
