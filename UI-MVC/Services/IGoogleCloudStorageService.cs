namespace IntegratieProject.UI.MVC.Services;

public interface IGoogleCloudStorageService
{
    Task<string> UploadProjectImageAsync(IFormFile file, string subplatformSlug);
}