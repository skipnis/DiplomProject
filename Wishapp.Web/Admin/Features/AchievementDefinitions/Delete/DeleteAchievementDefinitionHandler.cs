using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Admin.Features.AchievementDefinitions.Delete;

public sealed class DeleteAchievementDefinitionHandler(ApplicationDbContext db)
    : ICommandHandler<DeleteAchievementDefinitionCommand>
{
    public async Task<Result> HandleAsync(
        DeleteAchievementDefinitionCommand command,
        CancellationToken ct = default)
    {
        var definition = await db.AchievementDefinitions.FirstOrDefaultAsync(a => a.Id == command.Id, ct);
        if (definition is null)
            return Error.NotFound("Achievements.NotFound", "Achievement definition not found");

        db.AchievementDefinitions.Remove(definition);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
