using Microsoft.EntityFrameworkCore;
using Wishapp.Web.Common.Interfaces;
using Wishapp.Web.Common.Types;
using Wishapp.Web.Infrastructure.Database;
using Wishapp.Web.Infrastructure.Interfaces;
using Wishapp.Web.Infrastructure.Minio;

namespace Wishapp.Web.Admin.Features.Collections.UploadCover;

public sealed class UploadCollectionCoverHandler(
    ApplicationDbContext db,
    IStorageService storageService)
    : ICommandHandler<UploadCollectionCoverCommand, UploadCollectionCoverResponse>
{
    private const long MaxImageSize = 10 * 1024 * 1024;

    public async Task<Result<UploadCollectionCoverResponse>> HandleAsync(
        UploadCollectionCoverCommand command,
        CancellationToken ct = default)
    {
        var collection = await db.CatalogCollections.FirstOrDefaultAsync(c => c.Id == command.CollectionId, ct);

        if (collection is null)
            return Error.NotFound("Catalog.CollectionNotFound", "Collection not found");

        if (command.File.Length > MaxImageSize)
            return Error.Validation("Image.TooLarge", "Image must be less than 10MB");

        if (collection.CoverImagePath is not null)
            await storageService.DeleteAsync(collection.CoverImagePath, ct);

        var path = StoragePaths.CatalogCollectionCover(command.CollectionId);

        await using var stream = command.File.OpenReadStream();
        await storageService.UploadAsync(path, stream, command.File.ContentType, command.File.Length, ct);

        collection.SetCoverImage(path);
        await db.SaveChangesAsync(ct);

        return new UploadCollectionCoverResponse(path);
    }
}
