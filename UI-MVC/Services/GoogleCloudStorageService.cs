
using Google.Cloud.Storage.V1;

namespace IntegratieProject.UI.MVC.Services;

public class GoogleCloudStorageService : IGoogleCloudStorageService
{
    private readonly StorageClient _storageClient;
    private readonly IConfiguration _configuration;

    public GoogleCloudStorageService(IConfiguration configuration)
    {
        _configuration = configuration;
        _storageClient = StorageClient.Create();
    }

    public async Task<string> UploadProjectImageAsync(IFormFile file, string subplatformSlug)
    {
        var bucketName = _configuration["GoogleCloudStorage:BucketName"];
        var publicBaseUrl = _configuration["GoogleCloudStorage:PublicBaseUrl"];

        if (string.IsNullOrWhiteSpace(bucketName))
            throw new InvalidOperationException("GoogleCloudStorage:BucketName ontbreekt in appsettings.json.");

        if (string.IsNullOrWhiteSpace(publicBaseUrl))
            throw new InvalidOperationException("GoogleCloudStorage:PublicBaseUrl ontbreekt in appsettings.json.");

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
            throw new InvalidOperationException("Alleen jpg, jpeg, png of webp is toegestaan.");

        var fileName = $"{Guid.NewGuid()}{extension}";

        var objectName = $"projects/{subplatformSlug}/photos/{fileName}";

        await using var stream = file.OpenReadStream();

        await _storageClient.UploadObjectAsync(
            bucket: bucketName,
            objectName: objectName,
            contentType: file.ContentType,
            source: stream
        );

        return $"{publicBaseUrl}/{objectName}";
    }
}