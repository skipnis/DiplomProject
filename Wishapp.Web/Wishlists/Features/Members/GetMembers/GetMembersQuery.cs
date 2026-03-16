using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Wishlists.Dtos;

namespace Wishapp.Web.Wishlists.Features.Members.GetMembers;

public record GetMembersQuery(Guid WishlistId) : IQuery<List<WishlistMemberDto>>;