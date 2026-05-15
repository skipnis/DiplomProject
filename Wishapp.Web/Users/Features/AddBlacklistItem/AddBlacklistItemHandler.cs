using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Users.Entities;
using Wishapp.Web.Users.Features.GetMyBlacklist;

namespace Wishapp.Web.Users.Features.AddBlacklistItem;

public sealed class AddBlacklistItemHandler(ApplicationDbContext db)
    : ICommandHandler<AddBlacklistItemCommand, BlacklistItemResponse>
{
    private const int MaxBlacklistItems = 5;

    public async Task<Result<BlacklistItemResponse>> HandleAsync(
        AddBlacklistItemCommand command,
        CancellationToken ct = default)
    {
        var existingCount = await db.BlacklistItems
            .CountAsync(item => item.UserId == command.UserId, ct);

        if (existingCount >= MaxBlacklistItems)
            return Error.Failure("Blacklist.LimitReached", $"Maximum of {MaxBlacklistItems} blacklist items allowed");

        var blacklistItem = BlacklistItem.Create(command.UserId, command.Title);

        db.BlacklistItems.Add(blacklistItem);
        await db.SaveChangesAsync(ct);

        return new BlacklistItemResponse(blacklistItem.Id, blacklistItem.Title, blacklistItem.CreatedAt);
    }
}
