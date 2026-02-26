using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;

namespace ThrPresetsApi.Api.Infrastructure.S3;

public class S3Service(IAmazonS3 s3Client, IConfiguration config) : IS3Service
{
    private readonly string _bucketName = config["AWS:BucketName"]!;

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType)
    {
        await EnsureBucketExistsAsync();

        var key = $"presets/{Guid.NewGuid()}/{fileName}";
        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = fileStream,
            ContentType = contentType
        };

        await s3Client.PutObjectAsync(request);
        return key;
    }

    public async Task DeleteAsync(string key)
    {
        await s3Client.DeleteObjectAsync(_bucketName, key);
    }

    public string GetDownloadUrl(string key, string fileName)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = key,
            Expires = DateTime.UtcNow.AddMinutes(5),
            ResponseHeaderOverrides = new ResponseHeaderOverrides
            {
                ContentDisposition = $"attachment; filename=\"{fileName}\""
            }
        };

        return s3Client.GetPreSignedURL(request);
    }

    private async Task EnsureBucketExistsAsync()
    {
        if (!await AmazonS3Util.DoesS3BucketExistV2Async(s3Client, _bucketName))
        {
            await s3Client.PutBucketAsync(_bucketName);
        }
    }
}