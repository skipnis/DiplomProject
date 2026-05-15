using FluentValidation;

namespace Wishapp.Web.Users.Features.AddBlacklistItem;

public sealed class AddBlacklistItemRequestValidator : AbstractValidator<AddBlacklistItemRequest>
{
    public AddBlacklistItemRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(100);
    }
}
