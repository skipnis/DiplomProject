using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Infrastructure.Exceptions;

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

            services.AddAuthentication(configuration);

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

        private IServiceCollection AddAuthentication(IConfiguration configuration)
        {
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