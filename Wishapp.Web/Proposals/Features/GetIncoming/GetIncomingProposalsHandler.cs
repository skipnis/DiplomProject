using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Catalog;
using Wishapp.Web.Catalog.Dtos;
using Wishapp.Web.Common.Extensions;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Proposals.Dtos;
using Wishapp.Web.Proposals.Entities;
using Wishapp.Web.Wishlists;

namespace Wishapp.Web.Proposals.Features.GetIncoming;

public sealed class GetIncomingProposalsHandler(
    ApplicationDbContext db,
    ICatalogApi catalogApi,
    IWishlistsApi wishlistsApi)
    : IQueryHandler<GetIncomingProposalsQuery, PagedResponse<IncomingProposalDto>>
{
    public async Task<Result<PagedResponse<IncomingProposalDto>>> HandleAsync(
        GetIncomingProposalsQuery query,
        CancellationToken ct = default)
    {
        var proposals = await db.GiftProposals
            .AsNoTracking()
            .Where(p => p.RecipientId == query.UserId)
            .OrderBy(p => p.IsViewedByRecipient)
            .ThenByDescending(p => p.CreatedAt)
            .ToPagedResponseAsync(query.Request, ct);

        if (proposals.Items.Count == 0)
            return new PagedResponse<IncomingProposalDto>([], proposals.Page, proposals.PageSize, proposals.TotalCount);

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

        var items = proposals.Items.Select(p => MapToDto(p, catalogData, wishSummaries)).ToList();

        return new PagedResponse<IncomingProposalDto>(items, proposals.Page, proposals.PageSize, proposals.TotalCount);
    }

    private static IncomingProposalDto MapToDto(
        GiftProposal proposal,
        Dictionary<Guid, CatalogItemData> catalogData,
        Dictionary<Guid, Wishlists.Dtos.WishSummary> wishSummaries)
    {
        string? catalogItemName = null;
        string? catalogItemImagePath = null;
        decimal? catalogItemPrice = null;
        string? wishlistItemName = null;
        string? wishlistItemImagePath = null;

        if (proposal.SourceType == ProposalSourceType.Catalog && proposal.CatalogItemId.HasValue
            && catalogData.TryGetValue(proposal.CatalogItemId.Value, out var catalogEntry))
        {
            catalogItemName = catalogEntry.Name;
            catalogItemImagePath = catalogEntry.ImagePath;
            catalogItemPrice = catalogEntry.Price;
        }

        if (proposal.SourceType == ProposalSourceType.Wishlist && proposal.WishlistItemId.HasValue
            && wishSummaries.TryGetValue(proposal.WishlistItemId.Value, out var wishSummary))
        {
            wishlistItemName = wishSummary.WishName;
            wishlistItemImagePath = wishSummary.ImagePath;
        }

        return new IncomingProposalDto(
            proposal.Id,
            proposal.SourceType,
            proposal.SenderAlias ?? "Анонимный друг",
            proposal.HintMessage,
            proposal.IsViewedByRecipient,
            proposal.Status,
            proposal.RecipientComment,
            proposal.CreatedAt,
            proposal.ReactedAt,
            proposal.CatalogItemId,
            catalogItemName,
            catalogItemImagePath,
            catalogItemPrice,
            proposal.WishlistItemId,
            wishlistItemName,
            wishlistItemImagePath,
            proposal.CustomTitle,
            proposal.CustomDescription,
            proposal.CustomImagePath);
    }
}
