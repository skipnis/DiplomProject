using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Infrastructure.Authentication;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Infrastructure.Exceptions;
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

            services.AddHealthChecks(connectionString);

            services.AddAuthenticationInternal(configuration);

            services.AddAuthorizationInternal();

            services.AddHandlers();

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
                .AddNpgSql(connectionString);

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
                });
            
            return services;
        }

        private IServiceCollection AddAuthorizationInternal()
        {
            services.AddAuthorization();
            
            return services;
        }
        
        private IServiceCollection AdAuthorization()
        {
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
                .WithScopedLifetime());

            return services;
        }
    }
}