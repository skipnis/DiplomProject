namespace Wishapp.Web.Infrastructure.Interfaces;

public interface IStorageService
{
    Task<string> UploadAsync(string path, Stream stream, string contentType, long size, CancellationToken ct = default);
    Task DeleteAsync(string path, CancellationToken ct = default);
    Task CopyAsync(string sourcePath, string destPath, CancellationToken ct = default);
}