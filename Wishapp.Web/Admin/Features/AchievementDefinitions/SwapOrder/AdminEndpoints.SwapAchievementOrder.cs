using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Admin.Features.AchievementDefinitions.SwapOrder;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;

namespace Wishapp.Web.Admin;

public static partial class AdminEndpoints
{
    private static async Task<Results<NoContent, NotFound<Error>>> SwapAchievementOrder(
        [FromRoute] int id,
        SwapAchievementOrderCommand command,
        ICommandHandler<SwapAchievementOrderCommand> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(command with { Id = id }, ct);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : TypedResults.NotFound(result.Error);
    }
}
