using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Catalog.Entities;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Infrastructure.Interfaces;
using Wishapp.Web.Infrastructure.Minio;
using Wishapp.Web.Infrastructure.Parser;
using Wishapp.Web.Wishlists.Dtos;
using ZiggyCreatures.Caching.Fusion;

namespace Wishapp.Web.Admin.Features.Items.BatchImport;

public sealed class BatchImportCatalogItemsHandler(
    ApplicationDbContext db,
    IUrlParser urlParser,
    IStorageService storageService,
    IHttpClientFactory httpClientFactory,
    IFusionCache cache,
    ILogger<BatchImportCatalogItemsHandler> logger)
    : ICommandHandler<BatchImportCatalogItemsCommand, List<BatchImportItemResult>>
{
    private const long MaxImageSize = 10 * 1024 * 1024;

    public async Task<Result<List<BatchImportItemResult>>> HandleAsync(
        BatchImportCatalogItemsCommand command,
        CancellationToken ct = default)
    {
        var categoryExists = await db.CatalogCategories.AnyAsync(c => c.Id == command.CategoryId, ct);

        if (!categoryExists)
            return Error.NotFound("Catalog.CategoryNotFound", "Category not found");

        var distinctUrls = command.Urls
            .Select(url => url.Trim())
            .Where(url => !string.IsNullOrWhiteSpace(url))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var results = new List<BatchImportItemResult>(distinctUrls.Count);

        foreach (var url in distinctUrls)
        {
            var result = await ProcessUrlAsync(url, command.CategoryId, ct);
            results.Add(result);
        }

        if (results.Any(r => r.Status != BatchImportStatus.Failed))
            await cache.RemoveAsync("catalog:price-range", token: ct);

        return results;
    }

    private async Task<BatchImportItemResult> ProcessUrlAsync(string url, Guid categoryId, CancellationToken ct)
    {
        var urlValidation = UrlValidator.ValidatePageUrl(url);
        if (urlValidation.IsFailure)
            return Failed(url, urlValidation.Error.Description);

        ParsedWishData parsed;
        try
        {
            parsed = await urlParser.ParseAsync(url, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "URL parsing failed for {Url}", url);
            return Failed(url, "Failed to fetch or parse the URL");
        }

        if (string.IsNullOrWhiteSpace(parsed.Name))
            return Failed(url, "Could not extract product name from URL");

        var item = CatalogItem.Create(
            parsed.Name,
            parsed.Description,
            parsed.Price,
            currency: null,
            imagePath: null,
            url: url,
            categoryId: categoryId);

        try
        {
            db.CatalogItems.Add(item);
            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save catalog item for URL {Url}", url);
            db.Entry(item).State = EntityState.Detached;
            return Failed(url, "Database error while saving item");
        }

        if (!string.IsNullOrWhiteSpace(parsed.ExternalImageUrl))
        {
            try
            {
                var imagePath = StoragePaths.CatalogItemImage(item.Id);
                var uploadResult = await UploadFromUrlAsync(imagePath, parsed.ExternalImageUrl, ct);
                if (uploadResult.IsSuccess)
                {
                    item.SetImage(imagePath);
                    await db.SaveChangesAsync(ct);
                }
                else
                {
                    logger.LogWarning("Image upload failed for {Url}: {Error}", url, uploadResult.Error.Description);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Image upload threw for {Url}", url);
            }
        }

        var missingFields = new List<string>();
        if (string.IsNullOrWhiteSpace(parsed.Description)) missingFields.Add("Description");
        if (parsed.Price is null) missingFields.Add("Price");
        if (item.ImagePath is null) missingFields.Add("Image");

        var status = missingFields.Count == 0 ? BatchImportStatus.Success : BatchImportStatus.Partial;
        return new BatchImportItemResult(url, status, item.Id, missingFields, null);
    }

    private static BatchImportItemResult Failed(string url, string message) =>
        new(url, BatchImportStatus.Failed, null, [], message);

    private async Task<Result> UploadFromUrlAsync(string path, string imageUrl, CancellationToken ct)
    {
        var urlValidation = UrlValidator.ValidateImageUrl(imageUrl);
        if (urlValidation.IsFailure)
            return urlValidation.Error;

        var client = httpClientFactory.CreateClient("parser");

        using var request = new HttpRequestMessage(HttpMethod.Get, imageUrl);
        request.Headers.Add("Accept", "image/webp,image/*,*/*;q=0.8");

        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

        if (!response.IsSuccessStatusCode)
            return Error.Validation("Image.FetchFailed", $"Failed to download image: {(int)response.StatusCode}");

        var contentType = response.Content.Headers.ContentType?.MediaType;
        if (contentType is null || !contentType.StartsWith("image/"))
            return Error.Validation("Image.InvalidContentType", "URL must point to an image");

        if (response.Content.Headers.ContentLength > MaxImageSize)
            return Error.Validation("Image.TooLarge", "Image must be less than 10MB");

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        var size = response.Content.Headers.ContentLength ?? -1;

        await storageService.UploadAsync(path, stream, contentType, size, ct);
        return Result.Success();
    }
}
