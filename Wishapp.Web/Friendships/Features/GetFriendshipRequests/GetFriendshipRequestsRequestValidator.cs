using FluentValidation;

namespace Wishapp.Web.Friendships.Features.GetFriendshipRequests;

public sealed class GetFriendshipRequestsRequestValidator : AbstractValidator<GetFriendshipRequestsRequest>
{
    public GetFriendshipRequestsRequestValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum();

        RuleFor(x => x.Page)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
    }
}
