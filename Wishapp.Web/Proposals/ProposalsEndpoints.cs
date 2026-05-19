using Wishapp.Web.Infrastructure.Validation;
using Wishapp.Web.Proposals.Features.CreateProposal;
using Wishapp.Web.Proposals.Features.ReactToProposal;

namespace Wishapp.Web.Proposals;

public static partial class ProposalsEndpoints
{
    public static IEndpointRouteBuilder MapProposalsEndpoints(this IEndpointRouteBuilder app)
    {
        var proposals = app.MapGroup("/proposals").RequireAuthorization();

        proposals.MapPost("/", CreateProposal)
            .AddEndpointFilter<ValidationFilter<CreateProposalRequest>>();

        proposals.MapGet("/incoming", GetIncoming);

        proposals.MapGet("/outgoing", GetOutgoing);

        proposals.MapGet("/{id:guid}", GetProposalDetail);

        proposals.MapPatch("/{id:guid}/react", ReactToProposal)
            .AddEndpointFilter<ValidationFilter<ReactToProposalRequest>>();

        proposals.MapPost("/{id:guid}/image", UploadImage)
            .DisableAntiforgery();

        return app;
    }
}
