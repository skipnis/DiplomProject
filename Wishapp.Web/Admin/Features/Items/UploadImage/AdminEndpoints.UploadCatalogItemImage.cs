using Microsoft.AspNetCore.Http.HttpResults;
using Wishapp.Web.Admin.Features.Items.UploadImage;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;

namespace Wishapp.Web.Admin;

public static partial class AdminEndpoints
{
    private static async Task<Results<Ok<UploadCatalogItemImageResponse>, BadRequest<Error>, NotFound<Error>>> UploadCatalogItemImage(
        Guid id,
        [Microsoft.AspNetCore.Mvc.FromForm] UploadCatalogItemImageRequest request,
        ICommandHandler<UploadCatalogItemImageCommand, UploadCatalogItemImageResponse> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new UploadCatalogItemImageCommand(id, request.File, request.ExternalImageUrl), ct);

        if (!result.IsSuccess)
        {
            return result.Error.Type switch
            {
                ErrorType.NotFound => TypedResults.NotFound(result.Error),
                _ => TypedResults.BadRequest(result.Error)
            };
        }

        return TypedResults.Ok(result.Value);
    }
}
