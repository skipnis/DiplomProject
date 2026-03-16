using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Wishlists.Features.Members.RemoveMember;

public record RemoveMemberCommand(Guid WishlistId, Guid UserId) : ICommand;