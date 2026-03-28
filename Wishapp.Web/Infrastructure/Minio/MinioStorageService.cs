using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Wishapp.Web.Infrastructure.Interfaces;

namespace Wishapp.Web.Infrastructure.Minio;

public sealed class MinioStorageService(
    IMinioClient client,
    IOptions<MinioOptions> options) : IStorageService
{
    private readonly MinioOptions _options = options.Value;

    public async Task<string> UploadAsync(
        string path,
        Stream stream,
        string contentType,
        long size,
        CancellationToken ct = default)
    {
        var args = new PutObjectArgs()
            .WithBucket(_options.BucketName)
            .WithObject(path)
            .WithStreamData(stream)
            .WithObjectSize(size)
            .WithContentType(contentType);

        await client.PutObjectAsync(args, ct);

        return $"{_options.PublicUrl}/{_options.BucketName}/{path}";
    }

    public async Task DeleteAsync(string path, CancellationToken ct = default)
    {
        var args = new RemoveObjectArgs()
            .WithBucket(_options.BucketName)
            .WithObject(path);

        await client.RemoveObjectAsync(args, ct);
    }

    public async Task CopyAsync(string sourcePath, string destPath, CancellationToken ct = default)
    {
        var source = new CopySourceObjectArgs()
            .WithBucket(_options.BucketName)
            .WithObject(sourcePath);

        var args = new CopyObjectArgs()
            .WithBucket(_options.BucketName)
            .WithObject(destPath)
            .WithCopyObjectSource(source);

        await client.CopyObjectAsync(args, ct);
    }
}