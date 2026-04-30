using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Admin.Entities;
using Wishapp.Web.Gamification.Entities;

namespace Wishapp.Web.Infrastructure.Database;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        await SeedAdminUserAsync(db, config);
        await SeedCatalogBadgeDefinitionsAsync(db);
        await SeedFulfilledWishBadgeDefinitionsAsync(db);
        await SeedAchievementDefinitionsAsync(db);
    }

    private static async Task SeedAdminUserAsync(ApplicationDbContext db, IConfiguration config)
    {
        var username = config["Admin:Username"];
        var password = config["Admin:Password"];

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return;

        if (await db.AdminUsers.AnyAsync(a => a.Username == username))
            return;

        var hash = BCrypt.Net.BCrypt.HashPassword(password);
        db.AdminUsers.Add(AdminUser.Create(username, hash));
        await db.SaveChangesAsync();
    }

    private static async Task SeedCatalogBadgeDefinitionsAsync(ApplicationDbContext db)
    {
        if (await db.CatalogBadgeDefinitions.AnyAsync())
            return;

        db.CatalogBadgeDefinitions.AddRange(
            CatalogBadgeDefinition.Create("👥", "universal",     "Универсальный",          "Подойдёт почти любому"),
            CatalogBadgeDefinition.Create("🛠", "practical",     "Практичный",             "Пригодится в жизни"),
            CatalogBadgeDefinition.Create("🔥", "wow_effect",    "Вау-эффект",             "Удивляет с первого взгляда"),
            CatalogBadgeDefinition.Create("🔁", "everyday",      "Каждый день",            "Не осядет на полке"),
            CatalogBadgeDefinition.Create("💡", "unexpected",    "Сам бы не додумался",    "Неочевидная идея"),
            CatalogBadgeDefinition.Create("✨", "original",      "Оригинальный",           "Не банальный, с изюминкой"),
            CatalogBadgeDefinition.Create("🎯", "on_point",      "Попадание в интересы",   "Для тех, кто знает увлечения"),
            CatalogBadgeDefinition.Create("👤", "treat_yourself","Не купишь себе сам",     "Классика вишлиста"),
            CatalogBadgeDefinition.Create("🎂", "any_occasion",  "На любой повод",         "Не привязан к дате"),
            CatalogBadgeDefinition.Create("💎", "premium_feel",  "Ощущение люкса",         "Выглядит и ощущается дорого")
        );
        await db.SaveChangesAsync();
    }

    private static async Task SeedFulfilledWishBadgeDefinitionsAsync(ApplicationDbContext db)
    {
        if (await db.FulfilledWishBadgeDefinitions.AnyAsync())
            return;

        db.FulfilledWishBadgeDefinitions.AddRange(
            FulfilledWishBadgeDefinition.Create("🥹", "moved",         "Растрогал меня",           "Тронул до глубины души"),
            FulfilledWishBadgeDefinition.Create("🤩", "bragged",       "Уже всем похвастался",     "Захотелось показать друзьям"),
            FulfilledWishBadgeDefinition.Create("🫶", "felt_care",     "Почувствовал заботу",      "Видно что думали именно обо мне"),
            FulfilledWishBadgeDefinition.Create("🔁", "use_daily",     "Пользуюсь каждый день",    "Реально вошло в жизнь"),
            FulfilledWishBadgeDefinition.Create("💡", "surprised",     "Не ожидал такого",         "Приятно удивил"),
            FulfilledWishBadgeDefinition.Create("🎯", "hit_taste",     "Попал в мой вкус",         "Идеально совпало с характером"),
            FulfilledWishBadgeDefinition.Create("🕐", "memorable",     "Запомню надолго",          "Останется в памяти"),
            FulfilledWishBadgeDefinition.Create("🔥", "wow_moment",    "Вау, когда открыл",        "Реакция в момент распаковки"),
            FulfilledWishBadgeDefinition.Create("💬", "talk_about",    "Постоянно вспоминаю",      "Стал темой для разговоров"),
            FulfilledWishBadgeDefinition.Create("🌱", "changed_habit", "Изменил привычку",         "Открыл что-то новое для себя")
        );
        await db.SaveChangesAsync();
    }

    private static async Task SeedAchievementDefinitionsAsync(ApplicationDbContext db)
    {
        if (await db.AchievementDefinitions.AnyAsync())
            return;

        db.AchievementDefinitions.AddRange(
            AchievementDefinition.Create("Тронул до слёз",      "Получи бейдж «Растрогал меня» 3 раза",          "🥹", AchievementRuleType.SpecificBadgeCount, linkedBadgeTypeId: 1, threshold: 3,  order: 1),
            AchievementDefinition.Create("Мастер впечатлений",  "5 получателей похвастались подарком",            "🤩", AchievementRuleType.SpecificBadgeCount, linkedBadgeTypeId: 2, threshold: 5,  order: 2),
            AchievementDefinition.Create("Снайпер",             "10 раз получатель почувствовал заботу",          "🎯", AchievementRuleType.SpecificBadgeCount, linkedBadgeTypeId: 3, threshold: 10, order: 3),
            AchievementDefinition.Create("Нестандартный",       "5 подарков с неожиданной реакцией",              "💡", AchievementRuleType.SpecificBadgeCount, linkedBadgeTypeId: 5, threshold: 5,  order: 4),
            AchievementDefinition.Create("Идеальный даритель",  "Собери все 10 типов бейджей от получателей",     "🏆", AchievementRuleType.UniqueBadgeTypes,   linkedBadgeTypeId: null, threshold: 10, order: 5),
            AchievementDefinition.Create("Легенда вау-эффекта", "10 подарков, которые запомнились надолго",       "🔥", AchievementRuleType.SpecificBadgeCount, linkedBadgeTypeId: 7, threshold: 10, order: 6)
        );
        await db.SaveChangesAsync();
    }
}
