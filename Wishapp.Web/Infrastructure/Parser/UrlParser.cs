using System.Globalization;
using System.Text.Json.Nodes;
using AngleSharp;
using AngleSharp.Dom;
using Wishapp.Web.Infrastructure.Interfaces;
using Wishapp.Web.Wishlists.Dtos;

namespace Wishapp.Web.Infrastructure.Parser;

public sealed class UrlParser(IHttpClientFactory httpClientFactory, ILogger<UrlParser> logger) : IUrlParser
{
    private static class Selectors
    {
        public const string LdJson = "script[type='application/ld+json']";
        public const string OgTitle = "meta[property='og:title']";
        public const string OgDescription = "meta[property='og:description']";
        public const string OgImage = "meta[property='og:image']";
        public const string MetaTitle = "meta[name='title']";
        public const string MetaDescription = "meta[name='description']";
        public const string Title = "title";
        public const string ProductPrice = "meta[property='product:price:amount']";
        public const string ProductCurrency = "meta[property='product:price:currency']";
        public const string ItemPrice = "[itemprop='price']";
        public const string ItemCurrency = "[itemprop='priceCurrency']";
    }

    public async Task<ParsedWishData> ParseAsync(string url, CancellationToken ct = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient("parser");
            var html = await client.GetStringAsync(url, ct);

            var context = BrowsingContext.New(Configuration.Default);
            var document = await context.OpenAsync(req => req.Content(html), ct);

            var ldProduct = FindLdJsonProduct(document);

            var name =
                GetString(ldProduct?["name"])
                ?? document.QuerySelector(Selectors.OgTitle)?.GetAttribute("content")
                ?? document.QuerySelector(Selectors.Title)?.TextContent
                ?? document.QuerySelector(Selectors.MetaTitle)?.GetAttribute("content");

            var description =
                GetString(ldProduct?["description"])
                ?? document.QuerySelector(Selectors.OgDescription)?.GetAttribute("content")
                ?? document.QuerySelector(Selectors.MetaDescription)?.GetAttribute("content");

            var image =
                GetString(ldProduct?["image"])
                ?? document.QuerySelector(Selectors.OgImage)?.GetAttribute("content");

            var (price, currency) = ResolvePrice(ldProduct, document);

            if (name is null)
            {
                logger.LogWarning("Could not parse any data from {Url}", url);
            }

            return new ParsedWishData(name, description, price, currency, image);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to parse {Url}", url);

            return new ParsedWishData(null, null, null, null, null);
        }
    }

    private static JsonObject? FindLdJsonProduct(IDocument document)
    {
        foreach (var script in document.QuerySelectorAll(Selectors.LdJson))
        {
            JsonNode? parsed;

            try
            {
                parsed = JsonNode.Parse(script.TextContent);
            }
            catch
            {
                continue;
            }

            if (parsed is null)
            {
                continue;
            }

            IEnumerable<JsonNode> candidates = parsed is JsonArray rootArr
                ? rootArr.OfType<JsonNode>()
                : [parsed];

            foreach (var node in candidates)
            {
                if (node is JsonObject obj && IsProduct(obj["@type"]))
                {
                    return obj;
                }
            }
        }

        return null;
    }

    private static (decimal? price, string? currency) ResolvePrice(JsonObject? ldProduct, IDocument document)
    {
        if (ldProduct is not null)
        {
            var offersNode = ldProduct["offers"];
            
            var offers = offersNode is JsonArray arr ? arr[0] : offersNode;

            if (offers is not null)
            {
                var price = ParsePrice(GetString(offers["price"]) ?? GetString(offers["lowPrice"]));
                
                var currency = GetString(offers["priceCurrency"]);

                if (price is not null || currency is not null)
                {
                    return (price, currency);
                }
            }
        }

        var priceEl = document.QuerySelector(Selectors.ItemPrice);
        var currencyEl = document.QuerySelector(Selectors.ItemCurrency);

        var priceRaw =
            document.QuerySelector(Selectors.ProductPrice)?.GetAttribute("content")
            ?? priceEl?.GetAttribute("content")
            ?? priceEl?.TextContent;

        var currencyRaw =
            document.QuerySelector(Selectors.ProductCurrency)?.GetAttribute("content")
            ?? currencyEl?.GetAttribute("content")
            ?? currencyEl?.TextContent;

        return (ParsePrice(priceRaw), currencyRaw);
    }

    private static string? GetString(JsonNode? node) => node switch
    {
        JsonValue v when v.TryGetValue<string>(out var s) => s,
        JsonValue v => v.ToString(),
        JsonArray arr when arr.Count > 0 => GetString(arr[0]),
        JsonObject obj => GetString(obj["url"]) ?? GetString(obj["contentUrl"]),
        _ => null
    };

    private static bool IsProduct(JsonNode? typeNode) =>
        typeNode switch
        {
            JsonValue v when v.TryGetValue<string>(out var s) => s == "Product",
            JsonArray arr => arr.Any(n => GetString(n) == "Product"),
            _ => false
        };

    private static decimal? ParsePrice(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var span = raw.AsSpan();

        var count = 0;
        foreach (var c in span)
        {
            if (char.IsAsciiDigit(c) || c == '.' || c == ',')
            {
                count++;
            }
        }

        if (count == 0)
        {
            return null;
        }

        Span<char> filtered = stackalloc char[count];
        var wi = 0;
        var lastDotPos = -1;
        var lastCommaPos = -1;

        foreach (var c in span)
        {
            if (char.IsAsciiDigit(c))
            {
                filtered[wi++] = c;
            }
            else switch (c)
            {
                case '.':
                    lastDotPos = wi; filtered[wi++] = c;
                    break;
                case ',':
                    lastCommaPos = wi; filtered[wi++] = c;
                    break;
            }
        }

        Span<char> normBuf = stackalloc char[wi];
        var ni = 0;

        if (lastDotPos > lastCommaPos)
        {
            foreach (var c in filtered[..wi])
            {
                if (c != ',')
                {
                    normBuf[ni++] = c;
                }
            }
        }
        else if (lastCommaPos > lastDotPos)
        {
            foreach (var c in filtered[..wi])
            {
                if (c == '.')
                {
                    continue;
                }
                normBuf[ni++] = c == ',' ? '.' : c;
            }
        }
        else
        {
            filtered[..wi].CopyTo(normBuf);
            ni = wi;
        }

        return decimal.TryParse(normBuf[..ni], NumberStyles.Any, CultureInfo.InvariantCulture, out var p)
            ? p
            : null;
    }
}
