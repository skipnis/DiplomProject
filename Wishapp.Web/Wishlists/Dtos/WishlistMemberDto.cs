using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Wishlists.Dtos;

public record WishlistMemberDto(
    Guid UserId,
    WishlistMemberRole Role,
    string? CustomRoleName,
    DateTimeOffset JoinedAt)
{
    public static WishlistMemberDto From(WishlistMember member) => new(
        member.UserId,
        member.Role,
        member.CustomRoleName,
        member.JoinedAt);
}