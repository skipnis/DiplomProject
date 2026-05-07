using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Catalog;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Proposals.Dtos;
using Wishapp.Web.Proposals.Entities;
using Wishapp.Web.Users;
using Wishapp.Web.Wishlists;

namespace Wishapp.Web.Proposals.Features.GetProposalDetail;

public sealed class GetProposalDetailHandler(
    ApplicationDbContext db,
    ICatalogApi catalogApi,
    IWishlistsApi wishlistsApi,
    IUsersApi usersApi)
    : ICommandHandler<GetProposalDetailCommand, ProposalDetailDto>
{
    public async Task<Result<ProposalDetailDto>> HandleAsync(GetProposalDetailCommand query, CancellationToken ct = default)
    {
        var proposal = await db.GiftProposals
            .FirstOrDefaultAsync(p => p.Id == query.ProposalId, ct);

        if (proposal is null)
            return Error.NotFound("Proposals.NotFound", "Proposal not found");

        var isRecipient = proposal.RecipientId == query.UserId;
        var isSender = proposal.SenderId == query.UserId;

        if (!isRecipient && !isSender)
            return Error.Forbidden("Proposals.AccessDenied", "You do not have access to this proposal");

        if (isRecipient && !proposal.IsViewedByRecipient)
        {
            proposal.MarkViewed();
            await db.SaveChangesAsync(ct);
        }

        string? catalogItemName = null;
        string? catalogItemImagePath = null;
        decimal? catalogItemPrice = null;
        string? catalogItemUrl = null;
        string? wishlistItemName = null;
        string? wishlistItemImagePath = null;
        string? wishlistItemDescription = null;

        if (proposal.SourceType == ProposalSourceType.Catalog && proposal.CatalogItemId.HasValue)
        {
            var catalogData = await catalogApi.GetCatalogItemDataAsync(proposal.CatalogItemId.Value, ct);

            if (catalogData is not null)
            {
                catalogItemName = catalogData.Name;
                catalogItemImagePath = catalogData.ImagePath;
                catalogItemPrice = catalogData.Price;
                catalogItemUrl = catalogData.Url;
            }
        }

        if (proposal.SourceType == ProposalSourceType.Wishlist && proposal.WishlistItemId.HasValue)
        {
            var summaries = await wishlistsApi.GetWishesSummaryAsync([proposal.WishlistItemId.Value], ct);
            var summary = summaries.FirstOrDefault();

            if (summary is not null)
            {
                wishlistItemName = summary.WishName;
                wishlistItemImagePath = summary.ImagePath;
            }
        }

        Guid? recipientId = null;
        string? recipientDisplayName = null;
        string? recipientAvatarUrl = null;

        if (isSender)
        {
            var recipientInfo = await usersApi.GetUsersPublicInfoAsync([proposal.RecipientId], ct);
            var recipient = recipientInfo.GetValueOrDefault(proposal.RecipientId);
            recipientId = proposal.RecipientId;
            recipientDisplayName = recipient?.DisplayName;
            recipientAvatarUrl = recipient?.AvatarUrl;
        }

        return new ProposalDetailDto(
            proposal.Id,
            proposal.SourceType,
            proposal.SenderAlias ?? "Анонимный друг",
            proposal.HintMessage,
            proposal.IsViewedByRecipient,
            proposal.Status,
            proposal.RecipientComment,
            proposal.CreatedAt,
            proposal.ReactedAt,
            isSender,
            recipientId,
            recipientDisplayName,
            recipientAvatarUrl,
            proposal.CatalogItemId,
            catalogItemName,
            catalogItemImagePath,
            catalogItemPrice,
            catalogItemUrl,
            proposal.WishlistItemId,
            wishlistItemName,
            wishlistItemImagePath,
            wishlistItemDescription,
            proposal.CustomTitle,
            proposal.CustomDescription,
            proposal.CustomImagePath);
    }
}
