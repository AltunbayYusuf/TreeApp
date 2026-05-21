
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


    public async Task<string> UploadProjectMediaAsync(IFormFile file, string subplatformSlug)
    {
        var bucketName = _configuration["GoogleCloudStorage:BucketName"];
        var publicBaseUrl = _configuration["GoogleCloudStorage:PublicBaseUrl"];

        if (string.IsNullOrWhiteSpace(bucketName))
            throw new InvalidOperationException("GoogleCloudStorage:BucketName ontbreekt in appsettings.json.");

        if (string.IsNullOrWhiteSpace(publicBaseUrl))
            throw new InvalidOperationException("GoogleCloudStorage:PublicBaseUrl ontbreekt in appsettings.json.");

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".mp4", ".webm", ".mov" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
            throw new InvalidOperationException("Alleen jpg, jpeg, png, webp, mp4, webm of mov is toegestaan.");

        var fileName = $"{Guid.NewGuid()}{extension}";
        var objectName = $"projects/{subplatformSlug}/intro-media/{fileName}";

        await using var stream = file.OpenReadStream();

        await _storageClient.UploadObjectAsync(
            bucket: bucketName,
            objectName: objectName,
            contentType: file.ContentType,
            source: stream
        );

        return $"{publicBaseUrl}/{objectName}";
    }
    
    public async Task<string> UploadProjectMediaAsync(
        byte[] fileBytes,
        string fileName,
        string contentType,
        string subplatformSlug)
    {
        var bucketName = _configuration["GoogleCloudStorage:BucketName"];
        var publicBaseUrl = _configuration["GoogleCloudStorage:PublicBaseUrl"];

        if (string.IsNullOrWhiteSpace(bucketName))
            throw new InvalidOperationException("GoogleCloudStorage:BucketName ontbreekt in appsettings.json.");

        if (string.IsNullOrWhiteSpace(publicBaseUrl))
            throw new InvalidOperationException("GoogleCloudStorage:PublicBaseUrl ontbreekt in appsettings.json.");

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

        if (!allowedExtensions.Contains(extension))
            throw new InvalidOperationException("Alleen jpg, jpeg, png of webp is toegestaan voor AI-afbeeldingen.");

        var finalFileName = $"{Guid.NewGuid()}{extension}";
        var objectName = $"projects/{subplatformSlug}/intro-media/{finalFileName}";

        await using var stream = new MemoryStream(fileBytes);

        await _storageClient.UploadObjectAsync(
            bucket: bucketName,
            objectName: objectName,
            contentType: contentType,
            source: stream
        );

        return $"{publicBaseUrl}/{objectName}";
    }
}