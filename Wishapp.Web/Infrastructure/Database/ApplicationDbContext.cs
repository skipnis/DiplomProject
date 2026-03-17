using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Friendships.Entities;
using Wishapp.Web.Reservations.Entities;
using Wishapp.Web.Users.Entities;
using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Infrastructure.Database;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<AuthIdentity> AuthIdentities { get; set; }

    public DbSet<Friendship> Friendships { get; set; }

    public DbSet<Wishlist> Wishlists { get; set; }
    public DbSet<WishlistMember> WishlistMembers { get; set; }
    public DbSet<Wish> Wishes { get; set; }

    public DbSet<WishReservation> WishReservations { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}