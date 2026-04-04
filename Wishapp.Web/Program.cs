using HealthChecks.UI.Client;
using Wishapp.Web.Infrastructure.Database;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Scalar.AspNetCore;
using Serilog;
using Wishapp.Web;
using Wishapp.Web.Admin;
using Wishapp.Web.Catalog;
using Wishapp.Web.Events;
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
    .AddCatalogModule()
    .AddWishlistsModule()
    .AddReservationsModule()
    .AddEventsModule();

builder.Services.AddOpenApi();

builder.Services.AddCors();

var app = builder.Build();

app.UseCors(corsPolicyBuilder =>
{
    var allowedOrigin = app.Configuration["Cors:AllowedOrigin"] ?? "http://localhost:5173";
    corsPolicyBuilder.WithOrigins(allowedOrigin);
    corsPolicyBuilder.AllowAnyMethod();
    corsPolicyBuilder.AllowAnyHeader();
    corsPolicyBuilder.AllowCredentials();
});

app.UseExceptionHandler();

app.UseSerilogRequestLogging();

app.UseAuthentication();

app.UseAuthorization();

app.MapHealthChecks("/health",
    new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

app.MapUsersEndpoints()
    .MapFriendshipsEndpoints()
    .MapWishlistsEndpoints()
    .MapReservationsEndpoints()
    .MapEventsEndpoints()
    .MapCatalogEndpoints()
    .MapAdminEndpoints()
    .MapShareEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseApiDocumentation();
}

await DatabaseSeeder.SeedAsync(app.Services);

app.Run();