namespace ThrPresetsApi.Api.Infrastructure.S3;

public interface IS3Service
{
    Task<string> UploadAsync(Stream fileStream, string fileName, string contentType);
    Task DeleteAsync(string key);
    string GetDownloadUrl(string key, string fileName);
}