using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Interfaces;
using Wishapp.Web.Infrastructure.Parser;
using Wishapp.Web.Wishlists.Dtos;
using ZiggyCreatures.Caching.Fusion;

namespace Wishapp.Web.Wishlists.Features.Wishes.ParseWithUrl;

public sealed class ParseWishUrlHandler(IUrlParser urlParser, IFusionCache cache)
    : IQueryHandler<ParseWishUrlQuery, ParsedWishData>
{
    private static readonly Error ParseFailedError =
        Error.Failure("Parse.Failed", "Не удалось получить данные страницы");

    public async Task<Result<ParsedWishData>> HandleAsync(
        ParseWishUrlQuery query,
        CancellationToken ct = default)
    {
        var validationResult = UrlValidator.ValidatePageUrl(query.Url);

        if (validationResult.IsFailure)
        {
            return validationResult.Error;
        }

        var cacheKey = $"parse:{query.Url}";

        var cached = await cache.TryGetAsync<ParsedWishData>(cacheKey, token: ct);
        if (cached.HasValue)
        {
            return cached.Value.Name is not null
                ? cached.Value
                : ParseFailedError;
        }

        ParsedWishData parsed;
        try
        {
            parsed = await urlParser.ParseAsync(query.Url, ct);
        }
        catch (Exception)
        {
            return ParseFailedError;
        }

        if (parsed.Name is null)
        {
            await cache.SetAsync(cacheKey, parsed,
                new FusionCacheEntryOptions { Duration = TimeSpan.FromMinutes(10) },
                ct);
            return ParseFailedError;
        }

        await cache.SetAsync(cacheKey, parsed,
            new FusionCacheEntryOptions
            {
                Duration = TimeSpan.FromHours(6),
                IsFailSafeEnabled = true,
                FailSafeMaxDuration = TimeSpan.FromDays(1),
            },
            ct);

        return parsed;
    }
}
