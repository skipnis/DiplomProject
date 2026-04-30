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
    private static readonly Error NotFound = Error.NotFound("Wishes.NotFound", "Wish not found");
    private static readonly Error Forbidden = Error.Forbidden("Wishes.GiftBadges.Forbidden", "Only the wishlist owner can give gift badges");
    private static readonly Error NotFulfilled = Error.Failure("Wishes.GiftBadges.NotFulfilled", "Cannot rate a wish that has not been fulfilled");
    private static readonly Error NoGifter = Error.Failure("Wishes.GiftBadges.NoGifter", "This wish was not fulfilled by a reservation");
    private static readonly Error AlreadyRated = Error.Conflict("Wishes.GiftBadges.AlreadyRated", "Gift badges have already been given for this wish");
    private static readonly Error TooManyBadges = Error.Validation("Wishes.GiftBadges.TooMany", "Cannot give more than 3 badges per wish");

    public async Task<Result> HandleAsync(AddGiftBadgesCommand command, CancellationToken ct = default)
    {
        if (command.BadgeTypes.Count == 0 || command.BadgeTypes.Count > 3)
            return TooManyBadges;

        if (command.BadgeTypes.Distinct().Count() != command.BadgeTypes.Count)
            return Error.Validation("Wishes.GiftBadges.DuplicateBadges", "Badge types must be unique");

        var activeBadgeIds = await db.FulfilledWishBadgeDefinitions
            .Where(b => b.IsActive)
            .Select(b => b.Id)
            .ToListAsync(ct);

        var invalidBadges = command.BadgeTypes.Except(activeBadgeIds).ToList();
        if (invalidBadges.Count > 0)
            return Error.Validation("Wishes.GiftBadges.InvalidBadge", "One or more badge types are invalid or inactive");

        var eligibility = await wishlistsApi.GetGiftBadgeEligibilityAsync(command.WishlistId, command.WishId, ct);

        if (eligibility is null || !eligibility.WishExists)
            return NotFound;

        if (eligibility.WishlistOwnerId != command.UserId)
            return Forbidden;

        if (!eligibility.IsFulfilled)
            return NotFulfilled;

        if (!eligibility.FulfilledByReserverId.HasValue)
            return NoGifter;

        var alreadyRated = await db.FulfilledWishBadges
            .AnyAsync(b => b.WishId == command.WishId, ct);

        if (alreadyRated)
            return AlreadyRated;

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
