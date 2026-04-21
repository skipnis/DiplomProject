using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Admin.Features.Occasions.Create;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;

namespace Wishapp.Web.Admin;

public static partial class AdminEndpoints
{
    private static async Task<Results<Created<Guid>, Conflict<Error>>> CreateOccasion(
        CreateOccasionCommand command,
        ICommandHandler<CreateOccasionCommand, Guid> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(command, ct);

        if (!result.IsSuccess)
        {
            return TypedResults.Conflict(result.Error);
        }

        return TypedResults.Created($"/catalog/occasions/{result.Value}", result.Value);
    }
}
