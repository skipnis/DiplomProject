using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.RateLimiting;
using HealthChecks.UI.Client;
using Wishapp.Web.Infrastructure.Database;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Scalar.AspNetCore;
using Serilog;
using Wishapp.Web;
using Wishapp.Web.Admin;
using Wishapp.Web.Catalog;
using Wishapp.Web.Events;
using Wishapp.Web.Friendships;
using Wishapp.Web.Gamification;
using Wishapp.Web.Infrastructure;
using Wishapp.Web.Notifications;
using Wishapp.Web.Proposals;
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
    options.Limits.MaxRequestBodySize = 50 * 1024 * 1024;
});

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services
    .AddUsersModule()
    .AddFriendshipsModule()
    .AddCatalogModule()
    .AddGamificationModule(builder.Configuration)
    .AddWishlistsModule()
    .AddProposalsModule()
    .AddReservationsModule()
    .AddEventsModule()
    .AddNotificationsModule();

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("parse-url", o =>
    {
        o.Window = TimeSpan.FromMinutes(1);
        o.PermitLimit = 20;
        o.QueueLimit = 0;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

builder.Services.AddFusionCache();

builder.Services.AddOpenApi();

builder.Services.AddCors();

var app = builder.Build();

app.UseCors(corsPolicyBuilder =>
{
    var allowedOrigins = (app.Configuration["Cors:AllowedOrigins"] ?? app.Configuration["Cors:AllowedOrigin"] ?? "http://localhost:5173")
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    corsPolicyBuilder.WithOrigins(allowedOrigins);
    corsPolicyBuilder.AllowAnyMethod();
    corsPolicyBuilder.AllowAnyHeader();
    corsPolicyBuilder.AllowCredentials();
});

app.UseExceptionHandler();

app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (userId is not null)
            diagnosticContext.Set("UserId", userId);
    };
});

app.UseRateLimiter();

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
    .MapProposalsEndpoints()
    .MapReservationsEndpoints()
    .MapEventsEndpoints()
    .MapCatalogEndpoints()
    .MapGamificationEndpoints()
    .MapAdminEndpoints()
    .MapShareEndpoints()
    .MapNotificationsEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseApiDocumentation();
}

if (args.Contains("--seed"))
{
    await DatabaseSeeder.SeedAsync(app.Services);
    return;
}

app.Run();