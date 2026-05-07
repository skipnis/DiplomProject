using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Proposals.Features.UploadProposalImage;

public record UploadProposalImageCommand(
    Guid ProposalId,
    Guid UserId,
    IFormFile File) : ICommand<UploadProposalImageResponse>;

public record UploadProposalImageResponse(string ImagePath);
