namespace Wishapp.Web.Notifications.Entities;

public enum NotificationType
{
    WishReserved = 1,
    ReservationCancelled = 2,
    WishFulfilled = 3,

    FriendRequestReceived = 10,
    FriendRequestAccepted = 11,
    FriendRequestDeclined = 12,

    AddedToWishlist = 20,
    RemovedFromWishlist = 21,
    WishlistRoleUpdated = 22,

    ProposalReceived = 30,
    ProposalReacted = 31,

    AchievementEarned = 40,
}
