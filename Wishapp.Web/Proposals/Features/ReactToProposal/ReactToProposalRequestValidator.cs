using FluentValidation;
using Wishapp.Web.Proposals.Entities;

namespace Wishapp.Web.Proposals.Features.ReactToProposal;

public sealed class ReactToProposalRequestValidator : AbstractValidator<ReactToProposalRequest>
{
    public ReactToProposalRequestValidator()
    {
        RuleFor(x => x.Status)
            .Must(s => s is ProposalStatus.Liked or ProposalStatus.Disliked)
            .WithMessage("Status must be Liked or Disliked");

        RuleFor(x => x.Comment)
            .MaximumLength(500)
            .When(x => x.Comment is not null);
    }
}
