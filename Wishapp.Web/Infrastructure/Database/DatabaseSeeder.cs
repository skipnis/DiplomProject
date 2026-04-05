using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Admin.Entities;

namespace Wishapp.Web.Infrastructure.Database;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var username = config["Admin:Username"];
        var password = config["Admin:Password"];

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var exists = await db.AdminUsers.AnyAsync(a => a.Username == username);
        if (exists)
        {
            return;
        }

        var hash = BCrypt.Net.BCrypt.HashPassword(password);
        var admin = AdminUser.Create(username, hash);

        db.AdminUsers.Add(admin);
        await db.SaveChangesAsync();
    }
}
