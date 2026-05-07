using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Infrastructure.Interfaces;
using Wishapp.Web.Infrastructure.Minio;

namespace Wishapp.Web.Proposals;

public sealed class ProposalsApi(ApplicationDbContext db, IStorageService storageService) : IProposalsApi
{
    public async Task DeleteUserDataAsync(Guid userId, CancellationToken ct = default)
    {
        var imagePaths = await db.GiftProposals
            .AsNoTracking()
            .Where(p => (p.SenderId == userId || p.RecipientId == userId) && p.CustomImagePath != null)
            .Select(p => p.CustomImagePath!)
            .ToListAsync(ct);

        foreach (var path in imagePaths)
            await storageService.DeleteAsync(path, ct);

        await db.GiftProposals
            .Where(p => p.SenderId == userId || p.RecipientId == userId)
            .ExecuteDeleteAsync(ct);
    }
}
