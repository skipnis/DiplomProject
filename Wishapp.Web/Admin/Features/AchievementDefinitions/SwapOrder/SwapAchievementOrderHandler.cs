using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;

namespace Wishapp.Web.Admin.Features.AchievementDefinitions.SwapOrder;

public sealed class SwapAchievementOrderHandler(ApplicationDbContext db)
    : ICommandHandler<SwapAchievementOrderCommand>
{
    public async Task<Result> HandleAsync(SwapAchievementOrderCommand command, CancellationToken ct = default)
    {
        var achievement = await db.AchievementDefinitions.FirstOrDefaultAsync(a => a.Id == command.Id, ct);
        if (achievement is null)
            return Error.NotFound("Achievements.NotFound", "Achievement definition not found");

        var target = await db.AchievementDefinitions.FirstOrDefaultAsync(a => a.Id == command.TargetId, ct);
        if (target is null)
            return Error.NotFound("Achievements.NotFound", "Target achievement definition not found");

        var tempOrder = achievement.Order;
        achievement.SetOrder(target.Order);
        target.SetOrder(tempOrder);

        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
