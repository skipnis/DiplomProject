using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Admin.Features.CatalogBadgeDefinitions;
using Wishapp.Web.Gamification.Entities;
using Wishapp.Web.Gamification.Features.GetCatalogBadgeDefinitions;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Admin;

public static partial class AdminEndpoints
{
    private static async Task<Ok<List<CatalogBadgeDefinitionDto>>> GetAllCatalogBadgeDefinitions(
        ApplicationDbContext db,
        CancellationToken ct)
    {
        var items = await db.CatalogBadgeDefinitions
            .AsNoTracking()
            .OrderBy(b => b.Id)
            .Select(b => new CatalogBadgeDefinitionDto(b.Id, b.Emoji, b.Slug, b.Label, b.Description, b.IsActive))
            .ToListAsync(ct);

        return TypedResults.Ok(items);
    }

    private static async Task<Created<int>> CreateCatalogBadgeDefinition(
        [FromBody] CatalogBadgeDefinitionRequest request,
        ApplicationDbContext db,
        CancellationToken ct)
    {
        var definition = CatalogBadgeDefinition.Create(request.Emoji, request.Slug, request.Label, request.Description);
        db.CatalogBadgeDefinitions.Add(definition);
        await db.SaveChangesAsync(ct);
        return TypedResults.Created($"/admin/catalog/badge-definitions/{definition.Id}", definition.Id);
    }

    private static async Task<Results<Ok, NotFound>> UpdateCatalogBadgeDefinition(
        [FromRoute] int id,
        [FromBody] CatalogBadgeDefinitionRequest request,
        ApplicationDbContext db,
        CancellationToken ct)
    {
        var definition = await db.CatalogBadgeDefinitions.FirstOrDefaultAsync(b => b.Id == id, ct);
        if (definition is null)
            return TypedResults.NotFound();

        definition.Update(request.Emoji, request.Slug, request.Label, request.Description, request.IsActive);
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok();
    }

    private static async Task<Results<NoContent, NotFound>> DeleteCatalogBadgeDefinition(
        [FromRoute] int id,
        ApplicationDbContext db,
        CancellationToken ct)
    {
        var definition = await db.CatalogBadgeDefinitions.FirstOrDefaultAsync(b => b.Id == id, ct);
        if (definition is null)
            return TypedResults.NotFound();

        db.CatalogBadgeDefinitions.Remove(definition);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }
}
