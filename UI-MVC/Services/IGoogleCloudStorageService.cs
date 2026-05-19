namespace IntegratieProject.UI.MVC.Services;

public interface IGoogleCloudStorageService
{
    Task<string> UploadProjectMediaAsync(IFormFile file, string subplatformSlug);
}