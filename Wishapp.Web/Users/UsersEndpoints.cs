namespace Wishapp.Web.Users;

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var usersEndpoints = app.MapGroup("/users");
        return app;
    }
}
