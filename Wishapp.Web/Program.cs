using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using Wishapp.Web.Friendships;
using Wishapp.Web.Infrastructure;
using Wishapp.Web.Reservations;
using Wishapp.Web.Users;
using Wishapp.Web.Wishlists;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services);
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
});

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services
    .AddUsersModule()
    .AddFriendshipsModule()
    .AddWishlistsModule()
    .AddReservationsModule();

var app = builder.Build();

app.UseExceptionHandler();

app.UseSerilogRequestLogging();

app.MapHealthChecks("/health",
    new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

app.MapUsersEndpoints()
    .MapFriendshipsEndpoints()
    .MapWishlistsEndpoints()
    .MapReservationsEndpoints();

app.Run();