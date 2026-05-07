using FluentValidation;
using Wishapp.Web.Proposals.Entities;

namespace Wishapp.Web.Proposals.Features.CreateProposal;

public sealed class CreateProposalRequestValidator : AbstractValidator<CreateProposalRequest>
{
    public CreateProposalRequestValidator()
    {
        RuleFor(x => x.RecipientId).NotEmpty();
        RuleFor(x => x.SourceType).IsInEnum();

        RuleFor(x => x.CatalogItemId)
            .NotEmpty()
            .When(x => x.SourceType == ProposalSourceType.Catalog);

        RuleFor(x => x.WishlistItemId)
            .NotEmpty()
            .When(x => x.SourceType == ProposalSourceType.Wishlist);

        RuleFor(x => x.CustomTitle)
            .NotEmpty()
            .MaximumLength(200)
            .When(x => x.SourceType == ProposalSourceType.Custom);

        RuleFor(x => x.CustomDescription)
            .MaximumLength(2000)
            .When(x => x.CustomDescription is not null);

        RuleFor(x => x.HintMessage)
            .MaximumLength(500)
            .When(x => x.HintMessage is not null);

        RuleFor(x => x.SenderAlias)
            .MaximumLength(100)
            .When(x => x.SenderAlias is not null);
    }
}
