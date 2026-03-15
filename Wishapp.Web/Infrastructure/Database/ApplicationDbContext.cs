using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Friendships.Entities;
using Wishapp.Web.Users.Entities;

namespace Wishapp.Web.Infrastructure.Database;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<AuthIdentity> AuthIdentities { get; set; }
    public DbSet<Friendship> Friendships { get; set; } 
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}