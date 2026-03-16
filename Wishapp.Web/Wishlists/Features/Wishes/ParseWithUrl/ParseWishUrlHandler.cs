using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Interfaces;
using Wishapp.Web.Infrastructure.Parser;
using Wishapp.Web.Wishlists.Dtos;

namespace Wishapp.Web.Wishlists.Features.Wishes.ParseWithUrl;

public sealed class ParseWishUrlHandler(IUrlParser urlParser)
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

        var result = await urlParser.ParseAsync(query.Url, ct);

        return result;
    }
}