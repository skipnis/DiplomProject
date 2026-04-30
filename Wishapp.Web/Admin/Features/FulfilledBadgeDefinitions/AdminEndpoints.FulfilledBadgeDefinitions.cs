using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Admin.Features.FulfilledBadgeDefinitions;
using Wishapp.Web.Gamification.Entities;
using Wishapp.Web.Gamification.Features.GetFulfilledBadgeDefinitions;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Admin;

public static partial class AdminEndpoints
{
    private static async Task<Ok<List<FulfilledBadgeDefinitionDto>>> GetAllFulfilledBadgeDefinitions(
        ApplicationDbContext db,
        CancellationToken ct)
    {
        var items = await db.FulfilledWishBadgeDefinitions
            .AsNoTracking()
            .OrderBy(b => b.Id)
            .Select(b => new FulfilledBadgeDefinitionDto(b.Id, b.Emoji, b.Slug, b.Label, b.Description, b.IsActive))
            .ToListAsync(ct);

        return TypedResults.Ok(items);
    }

    private static async Task<Created<int>> CreateFulfilledBadgeDefinition(
        [FromBody] FulfilledBadgeDefinitionRequest request,
        ApplicationDbContext db,
        CancellationToken ct)
    {
        var definition = FulfilledWishBadgeDefinition.Create(request.Emoji, request.Slug, request.Label, request.Description);
        db.FulfilledWishBadgeDefinitions.Add(definition);
        await db.SaveChangesAsync(ct);
        return TypedResults.Created($"/admin/catalog/fulfilled-badge-definitions/{definition.Id}", definition.Id);
    }

    private static async Task<Results<Ok, NotFound>> UpdateFulfilledBadgeDefinition(
        [FromRoute] int id,
        [FromBody] FulfilledBadgeDefinitionRequest request,
        ApplicationDbContext db,
        CancellationToken ct)
    {
        var definition = await db.FulfilledWishBadgeDefinitions.FirstOrDefaultAsync(b => b.Id == id, ct);
        if (definition is null)
            return TypedResults.NotFound();

        definition.Update(request.Emoji, request.Slug, request.Label, request.Description, request.IsActive);
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok();
    }

    private static async Task<Results<NoContent, NotFound>> DeleteFulfilledBadgeDefinition(
        [FromRoute] int id,
        ApplicationDbContext db,
        CancellationToken ct)
    {
        var definition = await db.FulfilledWishBadgeDefinitions.FirstOrDefaultAsync(b => b.Id == id, ct);
        if (definition is null)
            return TypedResults.NotFound();

        db.FulfilledWishBadgeDefinitions.Remove(definition);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }
}
