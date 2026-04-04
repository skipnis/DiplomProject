using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Minio;
using Minio.AspNetCore.HealthChecks;
using Scalar.AspNetCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Infrastructure.Authentication;
using Wishapp.Web.Infrastructure.Authorization.Handlers;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Infrastructure.Exceptions;
using Wishapp.Web.Infrastructure.Interfaces;
using Wishapp.Web.Infrastructure.Minio;
using Wishapp.Web.Infrastructure.Parser;
using Wishapp.Web.Infrastructure.QrCode;
using Wishapp.Web.Users.Features.GoogleSignIn;

namespace Wishapp.Web.Infrastructure;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddInfrastructure(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Database")
                                   ?? throw new InvalidOperationException("Connection string 'Database' is not configured.");

            services.AddExceptionHandling();

            services.AddDatabase(connectionString);
            
            services.AddMinio(configuration);

            services.AddHealthChecks(connectionString);

            services.AddAuthenticationInternal(configuration);

            services.AddAuthorizationInternal();

            services.AddHandlers();

            services.AddParsing();

            services.AddQrCodeGeneration();

            services.AddApiDocumentation();
            
            return services;
        }

        private IServiceCollection AddDatabase(string connectionString)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString)
                    .UseSnakeCaseNamingConvention());
            
            return services;
        }

        private IServiceCollection AddHealthChecks(string connectionString)
        {
            services.AddHealthChecks()
                .AddNpgSql(connectionString)
                .AddMinio(sp => sp.GetRequiredService<IMinioClient>());

            return services;
        }

        private IServiceCollection AddExceptionHandling()
        {
            services.AddProblemDetails();
            
            services.AddExceptionHandler<GlobalExceptionHandler>();

            return services;
        }

        private IServiceCollection AddAuthenticationInternal(IConfiguration configuration)
        {
            services.AddScoped<ITokenProvider, TokenProvider>();
            
            services.AddScoped<IGoogleAuthService, GoogleAuthService>();
            
            services.AddOptions<JwtOptions>()
                .BindConfiguration(JwtOptions.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();
            
            services.AddOptions<GoogleOptions>()
                .BindConfiguration(GoogleOptions.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();
            
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()!;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtOptions.Issuer,
                        ValidAudience = jwtOptions.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtOptions.Secret))
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = ctx =>
                        {
                            if (ctx.Request.Cookies.TryGetValue("access_token", out var token))
                                ctx.Token = token;
                            
                            return Task.CompletedTask;
                        }
                    };
                });
            
            return services;
        }

        private IServiceCollection AddAuthorizationInternal()
        {
            services.AddAuthorization(options =>
                options.AddPolicy("Admin", policy => policy.RequireRole("admin")));

            services.AddScoped<IAuthorizationHandler, WishlistMemberAuthorizationHandler>();

            services.AddScoped<IAuthorizationHandler, WishlistFriendAuthorizationHandler>();

            return services;
        }
        
        private IServiceCollection AddMinio(IConfiguration configuration)
        {
            services.AddOptions<MinioOptions>()
                .BindConfiguration(MinioOptions.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            var minioOptions = configuration.GetSection(MinioOptions.SectionName).Get<MinioOptions>()!;

            services.AddMinio(configureClient => configureClient
                .WithEndpoint(minioOptions.Endpoint)
                .WithCredentials(minioOptions.AccessKey, minioOptions.SecretKey)
                .WithSSL(false)
                .Build());

            services.AddSingleton<IStorageService, MinioStorageService>();

            return services;
        }

        private IServiceCollection AddHandlers()
        {
            services.Scan(scan => scan
                .FromAssemblyOf<Program>()
                .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime()
                .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime()
                .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime()
                .AddClasses(classes => classes.AssignableTo(typeof(IValidator<>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            return services;
        }
        
        private IServiceCollection AddParsing()
        {
            services.AddHttpClient("parser", client =>
            {
                client.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/120.0.0.0 Safari/537.36");
            }).AddStandardResilienceHandler();

            services.AddScoped<IUrlParser, UrlParser>();

            return services;
        }
        
        private IServiceCollection AddQrCodeGeneration()
        {
            services.AddOptions<QrCodeOptions>()
                .BindConfiguration(QrCodeOptions.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddSingleton<IQrCodeService, QrCodeService>();

            return services;
        }
        
        private IServiceCollection AddApiDocumentation()
        {
            services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer((doc, context, ct) =>
                {
                    doc.Components ??= new OpenApiComponents();
                    
                    doc.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

                    doc.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT"
                    };

                    doc.Components.SecuritySchemes["OAuth2"] = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.OAuth2,
                        Flows = new OpenApiOAuthFlows
                        {
                            AuthorizationCode = new OpenApiOAuthFlow
                            {
                                AuthorizationUrl = new Uri("https://accounts.google.com/o/oauth2/v2/auth"),
                                TokenUrl = new Uri("https://oauth2.googleapis.com/token"),
                                Scopes = new Dictionary<string, string>
                                {
                                    ["openid"] = "OpenID",
                                    ["email"] = "Email",
                                    ["profile"] = "Profile"
                                }
                            }
                        }
                    };

                    return Task.CompletedTask;
                });
            });

            return services;
        }
    }

    extension(WebApplication app)
    {
        public WebApplication UseApiDocumentation()
        {
            app.MapOpenApi();

            app.MapScalarApiReference(options => options
                .AddPreferredSecuritySchemes("Bearer", "OAuth2")
                .AddHttpAuthentication("Bearer", auth => { })
                .AddAuthorizationCodeFlow("OAuth2", flow =>
                {
                    flow.ClientId = app.Configuration["Google:ClientId"]!;
                    flow.Pkce = Pkce.Sha256;
                    flow.SelectedScopes = ["openid", "email", "profile"];
                })
                .EnablePersistentAuthentication());

            return app;
        }
    }
}