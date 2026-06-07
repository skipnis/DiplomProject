using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Admin.Features.Categories.SwapOrder;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;

namespace Wishapp.Web.Admin;

public static partial class AdminEndpoints
{
    private static async Task<Results<NoContent, NotFound<Error>>> SwapCategoryOrder(
        [FromRoute] Guid id,
        SwapCategoryOrderCommand command,
        ICommandHandler<SwapCategoryOrderCommand> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(command with { Id = id }, ct);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : TypedResults.NotFound(result.Error);
    }
}
