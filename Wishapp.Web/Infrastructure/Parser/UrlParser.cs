using System.Globalization;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Dom;
using Wishapp.Web.Infrastructure.Interfaces;
using Wishapp.Web.Wishlists.Dtos;

namespace Wishapp.Web.Infrastructure.Parser;

public sealed class UrlParser(IHttpClientFactory httpClientFactory, ILogger<UrlParser> logger) : IUrlParser
{
    public async Task<ParsedWishData> ParseAsync(string url, CancellationToken ct = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient("parser");
            var html = await client.GetStringAsync(url, ct);

            var context = BrowsingContext.New(Configuration.Default);
            var document = await context.OpenAsync(req => req.Content(html), ct);

            var ldJson = TryParseLdJson(document);
            if (ldJson is not null) return ldJson;

            var og = TryParseOpenGraph(document);
            if (og is not null) return og;

            var meta = TryParseMeta(document);
            if (meta is not null) return meta;

            logger.LogWarning("Could not parse any data from {Url}", url);

            return new ParsedWishData(null, null, null, null, null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to parse {Url}", url);
            return new ParsedWishData(null, null, null, null, null);
        }
    }

    private static string? GetString(JsonNode? node) => node switch
    {
        JsonValue v when v.TryGetValue<string>(out var s) => s,
        JsonValue v => v.ToString(),
        JsonArray arr => GetString(arr[0]),
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
        if (string.IsNullOrWhiteSpace(raw)) return null;

        var digits = Regex.Replace(raw, @"[^\d.,]", "");
        if (string.IsNullOrEmpty(digits)) return null;

        var lastDot = digits.LastIndexOf('.');
        var lastComma = digits.LastIndexOf(',');

        string normalized;
        if (lastDot > lastComma)
        {
            normalized = digits.Replace(",", "");
        }
        else if (lastComma > lastDot)
        {
            normalized = digits.Replace(".", "").Replace(",", ".");
        }
        else
        {
            normalized = digits;
        }

        return decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out var p)
            ? p
            : null;
    }

    private static ParsedWishData? TryParseLdJson(IDocument document)
    {
        var scripts = document.QuerySelectorAll("script[type='application/ld+json']");

        foreach (var script in scripts)
        {
            JsonNode? parsed;
            try { parsed = JsonNode.Parse(script.TextContent); } catch { continue; }

            if (parsed is null) continue;

            IEnumerable<JsonNode> candidates = parsed is JsonArray rootArr
                ? rootArr.OfType<JsonNode>()
                : [parsed];

            foreach (var json in candidates)
            {
                if (json is not JsonObject obj) continue;

                if (!IsProduct(obj["@type"])) continue;

                var name = GetString(obj["name"]);
                if (name is null) continue;

                var description = GetString(obj["description"]);
                var image = GetString(obj["image"]);

                decimal? price = null;
                string? currency = null;

                var offersNode = obj["offers"];
                var offers = offersNode is JsonArray offersArr ? offersArr[0] : offersNode;

                if (offers is not null)
                {
                    var priceStr = GetString(offers["price"]) ?? GetString(offers["lowPrice"]);
                    price = ParsePrice(priceStr);
                    currency = GetString(offers["priceCurrency"]);
                }

                return new ParsedWishData(name, description, price, currency, image);
            }
        }

        return null;
    }

    private static ParsedWishData? TryParseOpenGraph(IDocument document)
    {
        var title = document.QuerySelector("meta[property='og:title']")
            ?.GetAttribute("content");

        if (title is null) return null;

        var description = document.QuerySelector("meta[property='og:description']")
            ?.GetAttribute("content");

        var image = document.QuerySelector("meta[property='og:image']")
            ?.GetAttribute("content");

        var priceAmount = document.QuerySelector("meta[property='product:price:amount']")
            ?.GetAttribute("content");

        var priceCurrency = document.QuerySelector("meta[property='product:price:currency']")
            ?.GetAttribute("content");

        priceAmount ??= document.QuerySelector("[itemprop='price']")
            ?.GetAttribute("content")
            ?? document.QuerySelector("[itemprop='price']")?.TextContent;

        priceCurrency ??= document.QuerySelector("[itemprop='priceCurrency']")
            ?.GetAttribute("content")
            ?? document.QuerySelector("[itemprop='priceCurrency']")?.TextContent;

        var price = ParsePrice(priceAmount);

        return new ParsedWishData(title, description, price, priceCurrency, image);
    }

    private static ParsedWishData? TryParseMeta(IDocument document)
    {
        var title = document.QuerySelector("title")?.TextContent
            ?? document.QuerySelector("meta[name='title']")?.GetAttribute("content");

        if (title is null) return null;

        var description = document.QuerySelector("meta[name='description']")
            ?.GetAttribute("content");

        return new ParsedWishData(title, description, null, null, null);
    }
}
