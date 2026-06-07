using Wishapp.Web.Catalog;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Notifications;
using Wishapp.Web.Notifications.Entities;
using Wishapp.Web.Proposals.Entities;
using Wishapp.Web.Users;
using Wishapp.Web.Wishlists;

namespace Wishapp.Web.Proposals.Features.CreateProposal;

public sealed class CreateProposalHandler(
    ApplicationDbContext db,
    IUsersApi usersApi,
    ICatalogApi catalogApi,
    IWishlistsApi wishlistsApi,
    INotificationsApi notificationsApi)
    : ICommandHandler<CreateProposalCommand, Guid>
{
    public async Task<Result<Guid>> HandleAsync(CreateProposalCommand command, CancellationToken ct = default)
    {
        if (command.SenderId == command.RecipientId)
            return Error.Validation("Proposals.SelfProposal", "Cannot send a proposal to yourself");

        var recipientExists = await usersApi.ExistsAsync(command.RecipientId, ct);

        if (recipientExists.IsFailure)
            return Error.NotFound("Proposals.RecipientNotFound", "Recipient not found");

        var validationError = await ValidateSourceAsync(command, ct);

        if (validationError is not null)
            return validationError;

        var proposal = GiftProposal.Create(
            command.SenderId,
            command.RecipientId,
            command.SourceType,
            command.CatalogItemId,
            command.WishlistItemId,
            command.CustomTitle,
            command.CustomDescription,
            command.HintMessage,
            command.SenderAlias);

        db.GiftProposals.Add(proposal);
        await db.SaveChangesAsync(ct);

        await notificationsApi.EnqueueAsync(command.RecipientId, NotificationType.ProposalReceived, new
        {
            proposalId = proposal.Id
        }, ct);

        return proposal.Id;
    }

    private async Task<Error?> ValidateSourceAsync(CreateProposalCommand command, CancellationToken ct)
    {
        switch (command.SourceType)
        {
            case ProposalSourceType.Catalog:
            {
                var itemExists = await catalogApi.ItemExistsAsync(command.CatalogItemId!.Value, ct);

                if (!itemExists)
                    return Error.NotFound("Proposals.CatalogItemNotFound", "Catalog item not found");

                return null;
            }

            case ProposalSourceType.Wishlist:
            {
                var summaries = await wishlistsApi.GetWishesSummaryAsync([command.WishlistItemId!.Value], ct);

                if (summaries.Count == 0)
                    return Error.NotFound("Proposals.WishNotFound", "Wish not found");

                return null;
            }

            default:
                return null;
        }
    }
}
