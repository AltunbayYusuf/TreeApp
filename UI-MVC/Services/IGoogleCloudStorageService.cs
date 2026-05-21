namespace IntegratieProject.UI.MVC.Services;

public interface IGoogleCloudStorageService
{
    Task<string> UploadProjectMediaAsync(IFormFile file, string subplatformSlug);
    Task<string> UploadProjectMediaAsync(
        byte[] fileBytes,
        string fileName,
        string contentType,
        string subplatformSlug);
}