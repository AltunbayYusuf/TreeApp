using Microsoft.AspNetCore.Hosting; 
using Microsoft.AspNetCore.Http; 
using IntegratieProject.BL.Domain.ideas;
using IntegratieProject.BL.Domain.project;
using IntegratieProject.BL.Domain.users;
using IntegratieProject.BL.interfaces;
using IntegratieProject.DAL.Identity;
using IntegratieProject.DAL.interfaces;

namespace IntegratieProject.BL;

public class SubplatformManager : ISubplatformManager
{
    private readonly ISubplatformRepository _subplatformRepository;
    private readonly IUserManager _userManager;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public SubplatformManager(
        ISubplatformRepository subplatformRepository,
        IUserManager userManager,
        IWebHostEnvironment webHostEnvironment) 
    {
        _subplatformRepository = subplatformRepository;
        _userManager = userManager;
        _webHostEnvironment = webHostEnvironment;
    }

    public SubPlatform GetSubPlatformBySlug(string slug)
    {
        return _subplatformRepository.ReadSubPlatformBySlug(slug);
    }

    public SubPlatform GetSubPlatform(int subPlatformId)
    {
        return _subplatformRepository.ReadSubPlatform(subPlatformId);
    }

    public void CreateSubPlatform(SubPlatform subPlatform)
    {
        _subplatformRepository.CreateSubPlatform(subPlatform);
    }

    public async Task<string> CreateSubPlatformAsync(string companyName, string slug, string adminEmail,
        IFormFile logoFile = null)
    {
        var generatedPassword = GenerateRandomPassword();

        var user = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            SubPlatformSlug = slug
        };
        var result = await _userManager.CreateUserAsync(user, generatedPassword);

        if (result.Succeeded)
        {
            var roleResult = await _userManager.AddUserToRoleAsync(user, "SubAdmin");
            if (!roleResult.Succeeded) throw new Exception("...");

            var rootPlatform = _subplatformRepository.ReadPlatform();

            var newSubPlatform = new SubPlatform
            {
                CompanyName = companyName,
                Slug = slug,
                Platform = rootPlatform,
            };

            if (logoFile != null && logoFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "logos");

                Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(logoFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await logoFile.CopyToAsync(fileStream);
                }

                newSubPlatform.Logo = new Media
                {
                    Uri = $"/images/logos/{uniqueFileName}"
                };
            }

            _subplatformRepository.CreateSubPlatform(newSubPlatform);

            var generalAdmin = _userManager.GetGeneralAdmin();

            if (generalAdmin == null)
            {
                throw new Exception("Kan geen SubAdmin aanmaken: er is nog geen GeneralAdmin in de database!");
            }

            var newSubAdmin = new SubAdmin
            {
                Name = companyName + " Admin",
                IdentityUserId = user.Id,
                SubPlatform = newSubPlatform,
                GeneralAdmin = generalAdmin
            };

            _subplatformRepository.CreateSubAdmin(newSubAdmin);

            return generatedPassword;
        }

        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        throw new Exception($"Kon subadmin account niet aanmaken. Reden: {errors}");
    }

    private string GenerateRandomPassword()
    {
        return Guid.NewGuid().ToString("N").Substring(0, 8) + "Aa1!";
    }

    public void UpdateSubPlatformLogo(string slug, string logoUri)
    {
        var subPlatform = _subplatformRepository.ReadSubPlatformBySlug(slug);

        if (subPlatform != null)
        {
            subPlatform.Logo = new Media { Uri = logoUri };

            _subplatformRepository.UpdateSubPlatform(subPlatform);
        }
    }
}