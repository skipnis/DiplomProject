using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Admin.Features.Collections.Create;
using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Admin;

public static partial class AdminEndpoints
{
    private static async Task<Created<Guid>> CreateCollection(
        CreateCollectionCommand command,
        ICommandHandler<CreateCollectionCommand, Guid> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(command, ct);
        return TypedResults.Created($"/catalog/collections/{result.Value}", result.Value);
    }
}
