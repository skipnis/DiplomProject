using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Gamification.Entities;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Wishlists;

namespace Wishapp.Web.Gamification.Features.AddGiftBadges;

public sealed class AddGiftBadgesHandler(ApplicationDbContext db, IWishlistsApi wishlistsApi)
    : ICommandHandler<AddGiftBadgesCommand>
{
    public async Task<Result> HandleAsync(AddGiftBadgesCommand command, CancellationToken ct = default)
    {
        var activeBadgeIds = await db.FulfilledWishBadgeDefinitions
            .Where(b => b.IsActive)
            .Select(b => b.Id)
            .ToListAsync(ct);

        var invalidBadges = command.BadgeTypes.Except(activeBadgeIds).ToList();
        if (invalidBadges.Count > 0)
            return Error.Validation("Wishes.GiftBadges.InvalidBadge", "One or more badge types are invalid or inactive");

        var eligibility = await wishlistsApi.GetGiftBadgeEligibilityAsync(command.WishlistId, command.WishId, ct);

        if (eligibility is null || !eligibility.WishExists)
            return Error.NotFound("Wishes.NotFound", "Wish not found");

        if (eligibility.WishlistOwnerId != command.UserId)
            return Error.Forbidden("Wishes.GiftBadges.Forbidden", "Only the wishlist owner can give gift badges");

        if (!eligibility.IsFulfilled)
            return Error.Failure("Wishes.GiftBadges.NotFulfilled", "Cannot rate a wish that has not been fulfilled");

        if (!eligibility.FulfilledByReserverId.HasValue)
            return Error.Failure("Wishes.GiftBadges.NoGifter", "This wish was not fulfilled by a reservation");

        var alreadyRated = await db.FulfilledWishBadges
            .AnyAsync(b => b.WishId == command.WishId, ct);

        if (alreadyRated)
            return Error.Conflict("Wishes.GiftBadges.AlreadyRated", "Gift badges have already been given for this wish");

        foreach (var badgeType in command.BadgeTypes)
        {
            db.FulfilledWishBadges.Add(FulfilledWishBadge.Create(
                command.WishId,
                command.UserId,
                eligibility.FulfilledByReserverId.Value,
                badgeType));
        }

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
