using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Wishlists.Dtos;

namespace Wishapp.Web.Wishlists.Features.Members;

public record AddMembersCommand(
    Guid WishlistId,
    Guid OwnerId,
    List<WishlistMemberInvite> Members) : ICommand;