using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Admin.Features.AchievementDefinitions.Update;

public sealed class UpdateAchievementDefinitionHandler(ApplicationDbContext db)
    : ICommandHandler<UpdateAchievementDefinitionCommand>
{
    public async Task<Result> HandleAsync(
        UpdateAchievementDefinitionCommand command,
        CancellationToken ct = default)
    {
        var definition = await db.AchievementDefinitions.FirstOrDefaultAsync(a => a.Id == command.Id, ct);
        if (definition is null)
            return Error.NotFound("Achievements.NotFound", "Achievement definition not found");

        definition.Update(
            command.Name, command.Description, command.Emoji,
            command.RuleType, command.LinkedBadgeTypeId,
            command.Threshold, command.Order, command.IsActive);

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
