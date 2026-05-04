using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wishapp.Web.Admin.Features.CatalogBadgeDefinitions;
using Wishapp.Web.Admin.Features.CatalogBadgeDefinitions.Create;
using Wishapp.Web.Admin.Features.CatalogBadgeDefinitions.Delete;
using Wishapp.Web.Admin.Features.CatalogBadgeDefinitions.GetAll;
using Wishapp.Web.Admin.Features.CatalogBadgeDefinitions.Update;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Gamification.Features.GetCatalogBadgeDefinitions;

namespace Wishapp.Web.Admin;

public static partial class AdminEndpoints
{
    private static async Task<Ok<List<CatalogBadgeDefinitionDto>>> GetAllCatalogBadgeDefinitions(
        IQueryHandler<GetAllCatalogBadgeDefinitionsQuery, List<CatalogBadgeDefinitionDto>> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new GetAllCatalogBadgeDefinitionsQuery(), ct);
        return TypedResults.Ok(result.Value);
    }

    private static async Task<Created<int>> CreateCatalogBadgeDefinition(
        [FromBody] CatalogBadgeDefinitionRequest request,
        ICommandHandler<CreateCatalogBadgeDefinitionCommand, int> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(
            new CreateCatalogBadgeDefinitionCommand(request.Emoji, request.Slug, request.Label, request.Description), ct);

        return TypedResults.Created($"/admin/catalog/badge-definitions/{result.Value}", result.Value);
    }

    private static async Task<Results<Ok, NotFound<Error>>> UpdateCatalogBadgeDefinition(
        [FromRoute] int id,
        [FromBody] CatalogBadgeDefinitionRequest request,
        ICommandHandler<UpdateCatalogBadgeDefinitionCommand> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(
            new UpdateCatalogBadgeDefinitionCommand(id, request.Emoji, request.Slug, request.Label, request.Description, request.IsActive), ct);

        if (!result.IsSuccess)
            return TypedResults.NotFound(result.Error);

        return TypedResults.Ok();
    }

    private static async Task<Results<NoContent, NotFound<Error>>> DeleteCatalogBadgeDefinition(
        [FromRoute] int id,
        ICommandHandler<DeleteCatalogBadgeDefinitionCommand> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new DeleteCatalogBadgeDefinitionCommand(id), ct);

        if (!result.IsSuccess)
            return TypedResults.NotFound(result.Error);

        return TypedResults.NoContent();
    }
}
