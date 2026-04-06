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
    public async Task<Result<ParsedWishData>> HandleAsync(
        ParseWishUrlQuery query,
        CancellationToken ct = default)
    {
        var validationResult = UrlValidator.ValidatePageUrl(query.Url);

        if (validationResult.IsFailure)
        {
            return validationResult.Error;
        }

        var result = await cache.GetOrSetAsync($"parse:{query.Url}",
            async token => await urlParser.ParseAsync(query.Url, token),
            new FusionCacheEntryOptions
            {
                Duration = TimeSpan.FromHours(6),
                IsFailSafeEnabled = true,
                FailSafeMaxDuration = TimeSpan.FromDays(1),
            },
            ct);

        return result;
    }
}
