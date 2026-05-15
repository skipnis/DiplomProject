using FluentValidation;

namespace Wishapp.Web.Users.Features.UpdateBlacklistItem;

public sealed class UpdateBlacklistItemRequestValidator : AbstractValidator<UpdateBlacklistItemRequest>
{
    public UpdateBlacklistItemRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(100);
    }
}
