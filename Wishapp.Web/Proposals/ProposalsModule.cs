namespace Wishapp.Web.Proposals;

public static class ProposalsModule
{
    public static IServiceCollection AddProposalsModule(this IServiceCollection services)
    {
        services.AddScoped<IProposalsApi, ProposalsApi>();

        return services;
    }
}
