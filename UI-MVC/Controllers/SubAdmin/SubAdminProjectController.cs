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
    private const string DraftProjectIdKey = "CreateProject_DraftProjectId";
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
        if (vm.PhotoUpload != null && vm.PhotoUpload.Length > 0)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(vm.PhotoUpload.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(nameof(vm.PhotoUpload), "Alleen jpg, jpeg, png of webp is toegestaan.");
                return View("ProjectInfo", vm);
            }

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "photos");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await vm.PhotoUpload.CopyToAsync(stream);

            vm.PhotoUri = $"/images/photos/{fileName}";
            
        }
        vm.PhotoUpload = null;

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

        if (vm.Topics == null)
        {
            vm.Topics = new List<IdeationTopicViewModel>();
        }
        
        if (!vm.Topics.Any())
            vm.Topics.Add(new IdeationTopicViewModel());
        
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
    
    [HttpGet]
    public async Task<IActionResult> EditDraft(string subplatform, int projectId)
    {
        var errorResult = await ValidateSubplatformAccess(subplatform);
        if (errorResult != null) return errorResult;

        var project = _projectManager.GetProject(projectId);

        if (project == null || project.SubPlatform.Slug != subplatform)
            return NotFound();

        if (project.Status != Status.Draft)
            return Forbid();

        SaveSession(DraftProjectIdKey, project.Id);

        SaveSession(InfoKey, new CreateProjectInfoViewModel
        {
            SubplatformSlug = subplatform,
            Name = project.Name,
            Introduction = project.Introduction,
            Type = project.Type,
            PhotoUri = project.Photo?.Uri
        });

        SaveSession(SurveyKey, new CreateProjecSurveyViewModel
        {
            SubplatformSlug = subplatform,
            Sections = project.QuestionList.Sections.Select(s => new SectionViewModel
            {
                Title = s.Title,
                Order = s.Order,
                Questions = s.Questions.Select(q => new QuestionViewModel
                {
                    Description = q.Description,
                    QuestionType = q.QuestionType,
                    RangeMin = q.RangeMin,
                    RangeMax = q.RangeMax,
                    Options = q.Options.Select(o => o.Text).ToList()
                }).ToList()
            }).ToList()
        });

        SaveSession(IdeationKey, new CreateProjectIdeationViewModel
        {
            SubplatformSlug = subplatform,
            SelectedEmojiGroup = project.ReactionEmojiGroup,
            Topics = project.Topics.Select(t => new IdeationTopicViewModel
            {
                Title = t.Theme,
                Description = t.Description
            }).ToList()
        });

        return RedirectToAction(nameof(ProjectInfo), new { subplatform });
    }
    
    [HttpGet]
    public async Task<IActionResult> CopyAsStartPoint(string subplatform, int projectId)
    {
        var errorResult = await ValidateSubplatformAccess(subplatform);
        if (errorResult != null) return errorResult;

        var project = _projectManager.GetProject(projectId);

        if (project == null || project.SubPlatform.Slug != subplatform)
            return NotFound();

        HttpContext.Session.Remove(DraftProjectIdKey);

        SaveSession(InfoKey, new CreateProjectInfoViewModel
        {
            SubplatformSlug = subplatform,
            Name = project.Name + " - kopie",
            Introduction = project.Introduction,
            Type = project.Type,
            PhotoUri = project.Photo?.Uri
        });

        SaveSession(SurveyKey, new CreateProjecSurveyViewModel
        {
            SubplatformSlug = subplatform,
            Sections = project.QuestionList?.Sections.Select(s => new SectionViewModel
            {
                Title = s.Title,
                Order = s.Order,
                Questions = s.Questions.Select(q => new QuestionViewModel
                {
                    Description = q.Description,
                    QuestionType = q.QuestionType,
                    RangeMin = q.RangeMin,
                    RangeMax = q.RangeMax,
                    Options = q.Options.Select(o => o.Text).ToList()
                }).ToList()
            }).ToList() ?? new List<SectionViewModel>()
        });

        SaveSession(IdeationKey, new CreateProjectIdeationViewModel
        {
            SubplatformSlug = subplatform,
            Topics = project.Topics.Select(t => new IdeationTopicViewModel
            {
                Title = t.Theme,
                Description = t.Description
            }).ToList()
        });

        return RedirectToAction(nameof(ProjectInfo), new { subplatform });
    }
    
private async Task<IActionResult> TryCreateProject(string subplatform)
{
    var validationResult = ValidateProjectSessions(subplatform);
    if (validationResult != null) return validationResult;

    var info = GetSession<CreateProjectInfoViewModel>(InfoKey)!;
    var survey = GetSession<CreateProjecSurveyViewModel>(SurveyKey)!;
    var ideation = GetSession<CreateProjectIdeationViewModel>(IdeationKey)!;

    var subPlatform = _subplatformManager.GetSubPlatformBySlug(subplatform);
    if (subPlatform == null) return NotFound();

    var draftProjectId = GetSession<int?>(DraftProjectIdKey);

    if (draftProjectId.HasValue)
    {
        return await UpdateExistingDraft(draftProjectId.Value, info, survey, ideation, subplatform);
    }

    var project = BuildProject(info, survey, ideation, subPlatform.Id);

    _projectManager.CreateProject(project);

    ClearProjectSessions();

    return RedirectToAction("Index", "SubAdmin", new { subplatform });
}

private IActionResult? ValidateProjectSessions(string subplatform)
{
    if (GetSession<CreateProjectInfoViewModel>(InfoKey) == null)
    {
        TempData["ProjectError"] = "Vul eerst Project Info volledig in.";
        return RedirectToAction(nameof(ProjectInfo), new { subplatform });
    }

    if (GetSession<CreateProjecSurveyViewModel>(SurveyKey) == null)
    {
        TempData["ProjectError"] = "Vul eerst Bevraging volledig in.";
        return RedirectToAction(nameof(CreateSurvey), new { subplatform });
    }

    if (GetSession<CreateProjectIdeationViewModel>(IdeationKey) == null)
    {
        TempData["ProjectError"] = "Vul eerst Ideation volledig in.";
        return RedirectToAction(nameof(CreateIdeation), new { subplatform });
    }

    return null;
}

private Project BuildProject(
    CreateProjectInfoViewModel info,
    CreateProjecSurveyViewModel survey,
    CreateProjectIdeationViewModel ideation,
    int subPlatformId)
{
    return new Project
    {
        Name = info.Name,
        Introduction = info.Introduction,
        Type = info.Type,
        ReactionEmojiGroup = ideation.SelectedEmojiGroup,
        Status = Status.Draft,
        SubPlatformId = subPlatformId,
        ReleaseDate = DateTime.UtcNow,
        Duration = 10,
        Photo = !string.IsNullOrWhiteSpace(info.PhotoUri)
            ? new Media { Uri = info.PhotoUri }
            : null,
        Topics = BuildTopics(ideation),
        QuestionList = BuildQuestionList(survey)
    };
}

private List<Topic> BuildTopics(CreateProjectIdeationViewModel ideation)
{
    return ideation.Topics.Select(t => new Topic
    {
        Theme = t.Title,
        Description = t.Description
    }).ToList();
}

private QuestionList BuildQuestionList(CreateProjecSurveyViewModel survey)
{
    return new QuestionList
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
    };
}

private async Task<IActionResult> UpdateExistingDraft(
    int draftProjectId,
    CreateProjectInfoViewModel info,
    CreateProjecSurveyViewModel survey,
    CreateProjectIdeationViewModel ideation,
    string subplatform)
{
    var existingProject = _projectManager.GetProject(draftProjectId);

    if (existingProject == null)
        return NotFound();

    existingProject.Name = info.Name;
    existingProject.Introduction = info.Introduction;
    existingProject.Type = info.Type;
    existingProject.ReactionEmojiGroup = ideation.SelectedEmojiGroup;
    existingProject.Photo = !string.IsNullOrWhiteSpace(info.PhotoUri)
        ? new Media { Uri = info.PhotoUri }
        : null;
    existingProject.Topics = BuildTopics(ideation);
    existingProject.QuestionList = BuildQuestionList(survey);

    _projectManager.UpdateProject(existingProject);

    ClearProjectSessions();

    return RedirectToAction("Index", "SubAdmin", new { subplatform });
}

private void ClearProjectSessions()
{
    HttpContext.Session.Remove(InfoKey);
    HttpContext.Session.Remove(SurveyKey);
    HttpContext.Session.Remove(IdeationKey);
    HttpContext.Session.Remove(DraftProjectIdKey);
}
  
}