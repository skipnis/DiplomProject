using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Infrastructure.Interfaces;
using Wishapp.Web.Infrastructure.ObjectStorage;
using Wishapp.Web.Proposals.Entities;

namespace Wishapp.Web.Proposals.Features.UploadProposalImage;

public sealed class UploadProposalImageHandler(
    ApplicationDbContext db,
    IStorageService storageService)
    : ICommandHandler<UploadProposalImageCommand, UploadProposalImageResponse>
{
    public async Task<Result<UploadProposalImageResponse>> HandleAsync(
        UploadProposalImageCommand command,
        CancellationToken ct = default)
    {
        if (command.File.Length > StorageLimits.MaxImageSizeBytes)
            return Error.Validation("Image.TooLarge", "Image must be less than 10MB");

        var proposal = await db.GiftProposals
            .FirstOrDefaultAsync(p => p.Id == command.ProposalId, ct);

        if (proposal is null)
            return Error.NotFound("Proposals.NotFound", "Proposal not found");

        if (proposal.SenderId != command.UserId)
            return Error.Forbidden("Proposals.AccessDenied", "Only the sender can upload an image for this proposal");

        if (proposal.SourceType != ProposalSourceType.Custom)
            return Error.Validation("Proposals.InvalidSource", "Images can only be uploaded for custom proposals");

        if (proposal.CustomImagePath is not null)
            await storageService.DeleteAsync(proposal.CustomImagePath, ct);

        var path = StoragePaths.ProposalCustomImage(command.ProposalId);

        await using var stream = command.File.OpenReadStream();
        await storageService.UploadAsync(path, stream, command.File.ContentType, command.File.Length, ct);

        proposal.SetCustomImage(path);
        await db.SaveChangesAsync(ct);

        return new UploadProposalImageResponse(path);
    }
}
