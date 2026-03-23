using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Admin.Features.Categories.Delete;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;

namespace Wishapp.Web.Admin;

public static partial class AdminEndpoints
{
    private static async Task<Results<NoContent, NotFound<Error>, Conflict<Error>>> DeleteCategory(
        [FromRoute] Guid id,
        ICommandHandler<DeleteCategoryCommand> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new DeleteCategoryCommand(id), ct);

        if (!result.IsSuccess)
        {
            return result.Error.Type == ErrorType.Conflict
                ? TypedResults.Conflict(result.Error)
                : TypedResults.NotFound(result.Error);
        }

        return TypedResults.NoContent();
    }
}
