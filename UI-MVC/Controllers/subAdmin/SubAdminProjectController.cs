using IntergratieProject.BL;
using IntergratieProject.DAL.Identity;
using IntergratieProject.Domain.ideas;
using IntergratieProject.Domain.project;
using IntergratieProject.UI.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IntergratieProject.UI.MVC.Controllers.subAdmin;

[Authorize(Roles = CustomIdentityConstants.SubAdminRoleName)]
public class SubAdminProjectsController : Controller
{
    private readonly IManager _manager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public SubAdminProjectsController(IManager manager, UserManager<ApplicationUser> userManager, IWebHostEnvironment webHostEnvironment)
    {
        _manager = manager;
        _userManager = userManager;
        _webHostEnvironment = webHostEnvironment;
    }

    private async Task<IActionResult> ValidateSubplatformAccess(string subplatform)
    {
        if (string.IsNullOrWhiteSpace(subplatform))
        {
            return NotFound();
        }

        var subPlatformEntity = _manager.GetSubPlatformBySlug(subplatform);
        if (subPlatformEntity == null)
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Redirect("/Identity/Account/Login");
        }

        if (!string.Equals(user.SubPlatformSlug, subplatform, StringComparison.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        return null;
    }

    [HttpGet]
    public async Task<IActionResult> ProjectInfo(string subplatform)
    {
        if (string.IsNullOrWhiteSpace(subplatform))
        {
            return NotFound();
        }

        var subPlatformEntity = _manager.GetSubPlatformBySlug(subplatform);
        if (subPlatformEntity == null)
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Redirect("/Identity/Account/Login");
        }

        if (!string.Equals(user.SubPlatformSlug, subplatform, StringComparison.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        var vm = new CreateProjectInfoViewModel
        {
            SubplatformSlug = subplatform,
            Type = ProjectType.VerticalScroll
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> CreateIdeation(string subplatform)
    {
        var errorResult = await ValidateSubplatformAccess(subplatform);
        if (errorResult != null)
        {
            return errorResult;
        }

        var vm = new CreateProjectIdeationViewModel
        {
            SubplatformSlug = subplatform
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveIdeation(string subplatform, CreateProjectIdeationViewModel vm)
    {
        var errorResult = await ValidateSubplatformAccess(subplatform);
        if (errorResult != null)
        {
            return errorResult;
        }

        if (!ModelState.IsValid)
        {
            vm.SubplatformSlug = subplatform;
            return View("CreateIdeation", vm);
        }

        vm.SubplatformSlug = subplatform;
        TempData["IdeationSavedMessage"] = "Ideation-instellingen zijn opgeslagen in de sessie. Persistente opslag is nog niet gekoppeld aan het projectdomein.";
        return RedirectToAction(nameof(CreateIdeation), new { subplatform });
    }
    
    // [HttpPost]
    // [ValidateAntiForgeryToken]
    // public async Task<IActionResult> Create(string subplatform, CreateProjectInfoViewModel vm)
    // {
    //     if (string.IsNullOrWhiteSpace(subplatform))
    //     {
    //         return NotFound();
    //     }
    //
    //     var subPlatformEntity = _manager.GetSubPlatformBySlug(subplatform);
    //     if (subPlatformEntity == null)
    //     {
    //         return NotFound();
    //     }
    //
    //     var user = await _userManager.GetUserAsync(User);
    //     if (user == null)
    //     {
    //         return Redirect("/Identity/Account/Login");
    //     }
    //
    //     if (!string.Equals(user.SubPlatformSlug, subplatform, StringComparison.OrdinalIgnoreCase))
    //     {
    //         return Forbid();
    //     }
    //
    //     if (!ModelState.IsValid)
    //     {
    //         vm.SubplatformSlug = subplatform;
    //         return View(vm);
    //     }
    //
    //     Media? uploadedPhoto = null;
    //
    //     if (vm.PhotoUpload != null && vm.PhotoUpload.Length > 0)
    //     {
    //         var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "photos");
    //         Directory.CreateDirectory(uploadsFolder);
    //
    //         var fileName = $"{Guid.NewGuid()}{Path.GetExtension(vm.PhotoUpload.FileName)}";
    //         var filePath = Path.Combine(uploadsFolder, fileName);
    //
    //         using (var stream = new FileStream(filePath, FileMode.Create))
    //         {
    //             await vm.PhotoUpload.CopyToAsync(stream);
    //         }
    //
    //         uploadedPhoto = new Media
    //         {
    //             Uri = $"/images/photos/{fileName}"
    //         };
    //     }
    //
    //     var project = new Project
    //     {
    //         Name = vm.Name,
    //         Introduction = vm.Introduction,
    //         Status = Status.Draft,
    //         Prompt = "",
    //         Duration = 10,
    //         ReleaseDate = DateTime.UtcNow,
    //         Type = vm.Type,
    //         SubPlatformId = subPlatformEntity.Id,
    //         Photo = uploadedPhoto ?? new Media { Uri = "/images/placeholders/project-placeholder.png" },
    //     };
    //
    //     _manager.CreateProject(project);
    //
    //     return RedirectToAction("Index", "SubAdmin", new { subplatform });
    // }
}
