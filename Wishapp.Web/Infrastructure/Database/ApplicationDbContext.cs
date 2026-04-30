using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Admin.Entities;
using Wishapp.Web.Catalog.Entities;
using Wishapp.Web.Events.Entities;
using Wishapp.Web.Friendships.Entities;
using Wishapp.Web.Gamification.Entities;
using Wishapp.Web.Notifications.Entities;
using Wishapp.Web.Reservations.Entities;
using Wishapp.Web.Users.Entities;
using Wishapp.Web.Wishlists.Entities;

namespace Wishapp.Web.Infrastructure.Database;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<AuthIdentity> AuthIdentities { get; set; }
    public DbSet<UserExternalToken> UserExternalTokens { get; set; }
    public DbSet<UserRefreshToken> RefreshTokens { get; set; }
    public DbSet<EmailOtp> EmailOtps { get; set; }

    public DbSet<Friendship> Friendships { get; set; }

    public DbSet<Wishlist> Wishlists { get; set; }
    public DbSet<WishlistMember> WishlistMembers { get; set; }
    public DbSet<Wish> Wishes { get; set; }

    public DbSet<WishReservation> WishReservations { get; set; }

    public DbSet<Event> Events { get; set; }

    public DbSet<Notification> Notifications { get; set; }

    public DbSet<AdminUser> AdminUsers { get; set; }
    public DbSet<CatalogCategory> CatalogCategories { get; set; }
    public DbSet<CatalogItem> CatalogItems { get; set; }
    public DbSet<CatalogCollection> CatalogCollections { get; set; }
    public DbSet<CatalogCollectionItem> CatalogCollectionItems { get; set; }
    public DbSet<CatalogOccasion> CatalogOccasions { get; set; }
    public DbSet<CatalogItemBadgeVote> CatalogItemBadgeVotes { get; set; }
    public DbSet<CatalogBadgeDefinition> CatalogBadgeDefinitions { get; set; }

    public DbSet<FulfilledWishBadge> FulfilledWishBadges { get; set; }
    public DbSet<FulfilledWishBadgeDefinition> FulfilledWishBadgeDefinitions { get; set; }

    public DbSet<UserAchievement> UserAchievements { get; set; }
    public DbSet<AchievementDefinition> AchievementDefinitions { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pg_trgm");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}