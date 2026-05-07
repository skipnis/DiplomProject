using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Catalog;
using Wishapp.Web.Common.Extensions;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Proposals.Dtos;
using Wishapp.Web.Proposals.Entities;
using Wishapp.Web.Users;
using Wishapp.Web.Wishlists;

namespace Wishapp.Web.Proposals.Features.GetOutgoing;

public sealed class GetOutgoingProposalsHandler(
    ApplicationDbContext db,
    ICatalogApi catalogApi,
    IWishlistsApi wishlistsApi,
    IUsersApi usersApi)
    : IQueryHandler<GetOutgoingProposalsQuery, PagedResponse<OutgoingProposalDto>>
{
    public async Task<Result<PagedResponse<OutgoingProposalDto>>> HandleAsync(
        GetOutgoingProposalsQuery query,
        CancellationToken ct = default)
    {
        var proposals = await db.GiftProposals
            .AsNoTracking()
            .Where(p => p.SenderId == query.UserId)
            .OrderByDescending(p => p.CreatedAt)
            .ToPagedResponseAsync(query.Request, ct);

        if (proposals.Items.Count == 0)
            return new PagedResponse<OutgoingProposalDto>([], proposals.Page, proposals.PageSize, proposals.TotalCount);

        var recipientIds = proposals.Items.Select(p => p.RecipientId).Distinct().ToList();
        var recipientInfo = await usersApi.GetUsersPublicInfoAsync(recipientIds, ct);

        var catalogIds = proposals.Items
            .Where(p => p.SourceType == ProposalSourceType.Catalog && p.CatalogItemId.HasValue)
            .Select(p => p.CatalogItemId!.Value)
            .Distinct()
            .ToList();

        var wishlistIds = proposals.Items
            .Where(p => p.SourceType == ProposalSourceType.Wishlist && p.WishlistItemId.HasValue)
            .Select(p => p.WishlistItemId!.Value)
            .Distinct()
            .ToList();

        var catalogData = catalogIds.Count > 0
            ? await catalogApi.GetCatalogItemsDataAsync(catalogIds, ct)
            : [];

        var wishSummaries = wishlistIds.Count > 0
            ? (await wishlistsApi.GetWishesSummaryAsync(wishlistIds, ct)).ToDictionary(s => s.WishId)
            : new Dictionary<Guid, Wishlists.Dtos.WishSummary>();

        var items = proposals.Items.Select(p =>
        {
            var recipient = recipientInfo.GetValueOrDefault(p.RecipientId);

            string? catalogItemName = null;
            string? catalogItemImagePath = null;
            decimal? catalogItemPrice = null;
            string? wishlistItemName = null;
            string? wishlistItemImagePath = null;

            if (p.SourceType == ProposalSourceType.Catalog && p.CatalogItemId.HasValue
                && catalogData.TryGetValue(p.CatalogItemId.Value, out var catalogEntry))
            {
                catalogItemName = catalogEntry.Name;
                catalogItemImagePath = catalogEntry.ImagePath;
                catalogItemPrice = catalogEntry.Price;
            }

            if (p.SourceType == ProposalSourceType.Wishlist && p.WishlistItemId.HasValue
                && wishSummaries.TryGetValue(p.WishlistItemId.Value, out var wishSummary))
            {
                wishlistItemName = wishSummary.WishName;
                wishlistItemImagePath = wishSummary.ImagePath;
            }

            return new OutgoingProposalDto(
                p.Id,
                p.SourceType,
                p.Status,
                p.HintMessage,
                p.RecipientComment,
                p.CreatedAt,
                p.ReactedAt,
                p.RecipientId,
                recipient?.DisplayName ?? "Unknown",
                recipient?.AvatarUrl,
                p.CatalogItemId,
                catalogItemName,
                catalogItemImagePath,
                catalogItemPrice,
                p.WishlistItemId,
                wishlistItemName,
                wishlistItemImagePath,
                p.CustomTitle,
                p.CustomDescription,
                p.CustomImagePath);
        }).ToList();

        return new PagedResponse<OutgoingProposalDto>(items, proposals.Page, proposals.PageSize, proposals.TotalCount);
    }

}
