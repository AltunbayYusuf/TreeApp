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

    public SubplatformManager(ISubplatformRepository subplatformRepository, IUserManager userManager)
    {
        _subplatformRepository = subplatformRepository;
        _userManager = userManager;
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

    public async Task<string> CreateSubPlatformAsync(string companyName, string slug, string adminEmail)
    {
        var generatedPassword = GenerateRandomPassword(); 
    
        var user = new ApplicationUser { UserName = adminEmail, Email = adminEmail ,EmailConfirmed = true};
        var result = await _userManager.CreateUserAsync(user, generatedPassword);
    
        if (result.Succeeded)
        {
            await _userManager.AddUserToRoleAsync(user, "SubAdmin");

            var newSubPlatform = new SubPlatform
            {
                CompanyName = companyName,
                Slug = slug
            };
            
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
}