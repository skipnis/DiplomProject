using Wishapp.Web.Wishlists.Dtos;

namespace Wishapp.Web.Wishlists.Features.Members;

public record AddMembersRequest(List<WishlistMemberInvite> Members);