using System.Text.Json;
using IntegratieProject.BL.Domain.ideas;
using IntegratieProject.BL.Domain.project;
using IntegratieProject.BL.Domain.questions;
using IntegratieProject.BL.interfaces;
using IntegratieProject.DAL.Identity;
using IntegratieProject.UI.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IntegratieProject.UI.MVC.Controllers.subAdmin;

[Authorize(Roles = CustomIdentityConstants.SubAdminRoleName)]
public class SubAdminProjectsController : Controller
{
    private readonly ISubplatformManager _subplatformManager;
    private readonly IProjectManager _projectManager;
    private readonly UserManager<ApplicationUser> _userManager;

    private const string InfoKey = "CreateProject_Info";
    private const string SurveyKey = "CreateProject_Survey";
    private const string IdeationKey = "CreateProject_Ideation";

    public SubAdminProjectsController(ISubplatformManager subplatformManager, IProjectManager projectManager, UserManager<ApplicationUser> userManager)
    {
        _subplatformManager = subplatformManager;
        _projectManager = projectManager;
        _userManager = userManager;
    }

    private async Task<IActionResult?> ValidateSubplatformAccess(string subplatform)
    {
        if (string.IsNullOrWhiteSpace(subplatform)) return NotFound();

        var subPlatformEntity = _subplatformManager.GetSubPlatformBySlug(subplatform);
        if (subPlatformEntity == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Redirect("/Identity/Account/Login");

        if (!string.Equals(user.SubPlatformSlug, subplatform, StringComparison.OrdinalIgnoreCase))
            return Forbid();

        return null;
    }

    private void SaveSession<T>(string key, T value)
    {
        HttpContext.Session.SetString(key, JsonSerializer.Serialize(value));
    }

    private T? GetSession<T>(string key)
    {
        var json = HttpContext.Session.GetString(key);
        return string.IsNullOrWhiteSpace(json) ? default : JsonSerializer.Deserialize<T>(json);
    }

    [HttpGet]
    public async Task<IActionResult> ProjectInfo(string subplatform)
    {
        var errorResult = await ValidateSubplatformAccess(subplatform);
        if (errorResult != null) return errorResult;

        var vm = GetSession<CreateProjectInfoViewModel>(InfoKey) ?? new CreateProjectInfoViewModel
        {
            SubplatformSlug = subplatform,
            Type = ProjectType.VerticalScroll
        };

        vm.SubplatformSlug = subplatform;
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string subplatform, CreateProjectInfoViewModel vm)
    {
        var errorResult = await ValidateSubplatformAccess(subplatform);
        if (errorResult != null) return errorResult;

        vm.SubplatformSlug = subplatform;

        if (!ModelState.IsValid)
            return View("ProjectInfo", vm);

        SaveSession(InfoKey, vm);

        return await TryCreateProject(subplatform);
    }

    [HttpGet]
    public async Task<IActionResult> CreateSurvey(string subplatform)
    {
        var errorResult = await ValidateSubplatformAccess(subplatform);
        if (errorResult != null) return errorResult;

        var vm = GetSession<CreateProjecSurveyViewModel>(SurveyKey) ?? new CreateProjecSurveyViewModel();
        vm.SubplatformSlug = subplatform;

        return View(vm);
    }
    
    [HttpGet]
    public async Task<IActionResult> CreateIdeation(string subplatform)
    {
        var errorResult = await ValidateSubplatformAccess(subplatform);
        if (errorResult != null) return errorResult;

        var vm = GetSession<CreateProjectIdeationViewModel>(IdeationKey) ?? new CreateProjectIdeationViewModel();
        vm.SubplatformSlug = subplatform;

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveIdeation(string subplatform, CreateProjectIdeationViewModel vm)
    {
        var errorResult = await ValidateSubplatformAccess(subplatform);
        if (errorResult != null) return errorResult;

        vm.SubplatformSlug = subplatform;

        vm.Topics = vm.Topics
            .Where(t => !string.IsNullOrWhiteSpace(t.Title))
            .ToList();

        if (!vm.Topics.Any())
        {
            ModelState.AddModelError("", "Je moet minstens 1 topic invullen.");
        }

        if (!ModelState.IsValid)
            return View("CreateIdeation", vm);

        SaveSession(IdeationKey, vm);

        return await TryCreateProject(subplatform);
    }

    private async Task<IActionResult> TryCreateProject(string subplatform)
    {
        var info = GetSession<CreateProjectInfoViewModel>(InfoKey);
        var survey = GetSession<CreateProjecSurveyViewModel>(SurveyKey);
        var ideation = GetSession<CreateProjectIdeationViewModel>(IdeationKey);

        if (info == null)
        {
            TempData["ProjectError"] = "Vul eerst Project Info volledig in.";
            return RedirectToAction(nameof(ProjectInfo), new { subplatform });
        }

        if (survey == null)
        {
            TempData["ProjectError"] = "Vul eerst Bevraging volledig in.";
            return RedirectToAction(nameof(CreateSurvey), new { subplatform });
        }

        if (ideation == null)
        {
            TempData["ProjectError"] = "Vul eerst Ideation volledig in.";
            return RedirectToAction(nameof(CreateIdeation), new { subplatform });
        }

        var subPlatform = _subplatformManager.GetSubPlatformBySlug(subplatform);
        if (subPlatform == null) return NotFound();

        var project = new Project
        {
            Name = info.Name,
            Introduction = info.Introduction,
            Type = info.Type,
            Status = Status.Draft,
            SubPlatformId = subPlatform.Id,
            ReleaseDate = DateTime.UtcNow,
            Duration = 10,

            Topics = ideation.Topics.Select(t => new Topic
            {
                Theme = t.Title,
                Description = t.Description
            }).ToList(),

            QuestionList = new QuestionList
            {
                Sections = survey.Sections.Select((s, sectionIndex) => new Section
                {
                    Title = s.Title,
                    Order = sectionIndex + 1,
                    Questions = s.Questions.Select(q => new Question
                    {
                        Description = q.Description,
                        QuestionType = q.QuestionType,
                        RangeMin = q.RangeMin,
                        RangeMax = q.RangeMax,
                        Options = q.Options
                            .Where(o => !string.IsNullOrWhiteSpace(o))
                            .Select(o => new QuestionOption { Text = o })
                            .ToList()
                    }).ToList()
                }).ToList()
            }
        };

        _projectManager.CreateProject(project);
        HttpContext.Session.Remove(InfoKey);
        HttpContext.Session.Remove(SurveyKey);
        HttpContext.Session.Remove(IdeationKey);

        return RedirectToAction("Index", "SubAdmin", new { subplatform });
    }
    
  
}