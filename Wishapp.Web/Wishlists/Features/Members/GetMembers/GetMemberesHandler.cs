using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Wishlists.Dtos;

namespace Wishapp.Web.Wishlists.Features.Members.GetMembers;

public sealed class GetMembersHandler(ApplicationDbContext db)
    : IQueryHandler<GetMembersQuery, List<WishlistMemberDto>>
{
    public async Task<Result<List<WishlistMemberDto>>> HandleAsync(
        GetMembersQuery query,
        CancellationToken ct = default)
    {
        var members = await db.WishlistMembers
            .AsNoTracking()
            .Where(m => m.WishlistId == query.WishlistId)
            .Select(m => new WishlistMemberDto(
                m.UserId,
                m.Role,
                m.CustomRoleName,
                m.JoinedAt))
            .ToListAsync(ct);

        return members;
    }
}