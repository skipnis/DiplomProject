using FluentValidation;
using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Wishlists.Features.Members.UpdateMemberRole;

public sealed class UpdateMemberRoleRequestValidator : AbstractValidator<UpdateMemberRoleRequest>
{
    public UpdateMemberRoleRequestValidator()
    {
        RuleFor(x => x.Role)
            .IsInEnum()
            .NotEqual(WishlistMemberRole.Owner);

        RuleFor(x => x.CustomRoleName)
            .MaximumLength(50)
            .When(x => x.CustomRoleName is not null);
    }
}
