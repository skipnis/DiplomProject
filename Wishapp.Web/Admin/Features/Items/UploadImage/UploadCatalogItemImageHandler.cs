using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Infrastructure.Interfaces;
using Wishapp.Web.Infrastructure.ObjectStorage;
using Wishapp.Web.Common;
using Wishapp.Web.Infrastructure.Parser;

namespace Wishapp.Web.Admin.Features.Items.UploadImage;

public sealed class UploadCatalogItemImageHandler(
    ApplicationDbContext db,
    IStorageService storageService,
    IHttpClientFactory httpClientFactory)
    : ICommandHandler<UploadCatalogItemImageCommand, UploadCatalogItemImageResponse>
{
    public async Task<Result<UploadCatalogItemImageResponse>> HandleAsync(
        UploadCatalogItemImageCommand command,
        CancellationToken ct = default)
    {
        if (command.File is null && command.ExternalImageUrl is null)
            return Error.Validation("Image.Required", "File or ExternalImageUrl must be provided");

        if (command.File is not null && command.ExternalImageUrl is not null)
            return Error.Validation("Image.Conflict", "Provide either File or ExternalImageUrl, not both");

        var item = await db.CatalogItems.FirstOrDefaultAsync(i => i.Id == command.ItemId, ct);

        if (item is null)
            return Error.NotFound("Catalog.ItemNotFound", "Item not found");

        if (item.ImagePath is not null)
            await storageService.DeleteAsync(item.ImagePath, ct);

        var path = StoragePaths.CatalogItemImage(command.ItemId);

        var uploadResult = command.File is not null
            ? await UploadFromFileAsync(path, command.File, ct)
            : await UploadFromUrlAsync(path, command.ExternalImageUrl!, ct);

        if (uploadResult.IsFailure)
            return uploadResult.Error;

        item.SetImage(path);
        await db.SaveChangesAsync(ct);

        return new UploadCatalogItemImageResponse(path);
    }

    private async Task<Result> UploadFromFileAsync(string path, IFormFile file, CancellationToken ct)
    {
        if (file.Length > StorageLimits.MaxImageSizeBytes)
            return Error.Validation("Image.TooLarge", "Image must be less than 10MB");

        await using var stream = file.OpenReadStream();
        await storageService.UploadAsync(path, stream, file.ContentType, file.Length, ct);

        return Result.Success();
    }

    private async Task<Result> UploadFromUrlAsync(string path, string url, CancellationToken ct)
    {
        var urlValidation = UrlValidator.ValidateImageUrl(url);

        if (urlValidation.IsFailure)
            return urlValidation.Error;

        var client = httpClientFactory.CreateClient("parser");

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Accept", "image/webp,image/*,*/*;q=0.8");

        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

        if (!response.IsSuccessStatusCode)
            return Error.Validation("Image.FetchFailed", $"Failed to download image: {(int)response.StatusCode}");

        var contentType = response.Content.Headers.ContentType?.MediaType;

        if (contentType is null || !contentType.StartsWith("image/"))
            return Error.Validation("Image.InvalidContentType", "URL must point to an image");

        if (response.Content.Headers.ContentLength > StorageLimits.MaxImageSizeBytes)
            return Error.Validation("Image.TooLarge", "Image must be less than 10MB");

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        var size = response.Content.Headers.ContentLength ?? -1;

        await storageService.UploadAsync(path, stream, contentType, size, ct);

        return Result.Success();
    }
}
