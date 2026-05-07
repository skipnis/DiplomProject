using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Notifications;
using Wishapp.Web.Notifications.Entities;
using Wishapp.Web.Proposals.Entities;

namespace Wishapp.Web.Proposals.Features.ReactToProposal;

public sealed class ReactToProposalHandler(
    ApplicationDbContext db,
    INotificationsApi notificationsApi)
    : ICommandHandler<ReactToProposalCommand>
{
    public async Task<Result> HandleAsync(ReactToProposalCommand command, CancellationToken ct = default)
    {
        var proposal = await db.GiftProposals
            .FirstOrDefaultAsync(p => p.Id == command.ProposalId, ct);

        if (proposal is null)
            return Error.NotFound("Proposals.NotFound", "Proposal not found");

        if (proposal.RecipientId != command.UserId)
            return Error.Forbidden("Proposals.AccessDenied", "Only the recipient can react to a proposal");

        if (proposal.Status != ProposalStatus.Pending)
            return Error.Conflict("Proposals.AlreadyReacted", "Proposal has already been reacted to");

        proposal.React(command.Status, command.Comment);
        await db.SaveChangesAsync(ct);

        await notificationsApi.EnqueueAsync(proposal.SenderId, NotificationType.ProposalReacted, new
        {
            proposalId = proposal.Id,
            status = command.Status.ToString()
        }, ct);

        return Result.Success();
    }
}
