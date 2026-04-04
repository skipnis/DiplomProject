using FluentValidation;

namespace Wishapp.Web.Wishlists.Features.Members;

public sealed class AddMembersRequestValidator : AbstractValidator<AddMembersRequest>
{
    public AddMembersRequestValidator()
    {
        RuleFor(x => x.Members)
            .NotEmpty();
    }
}
