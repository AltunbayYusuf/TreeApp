using System.Text.Json;
using IntegratieProject.BL.Domain.ideas;
using IntegratieProject.BL.Domain.project;
using IntegratieProject.BL.Domain.questions;
using IntegratieProject.BL.Domain.Questions;
using IntegratieProject.BL.interfaces;
using IntegratieProject.BL.Interfaces;
using IntegratieProject.DAL.Identity;
using IntegratieProject.UI.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using IntegratieProject.UI.MVC.Services;
using Microsoft.AspNetCore.RateLimiting;

namespace IntegratieProject.UI.MVC.Controllers.subAdmin;

[Authorize(Roles = CustomIdentityConstants.SubAdminRoleName)]
public class SubAdminProjectsController : Controller
{
    private readonly ISubplatformManager _subplatformManager;
    private readonly IProjectManager _projectManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IImageGenerationService _imageGenerationService;
    private readonly IIntroTextService _introTextService;
    private readonly IAiSurveyGenerationService _aiSurveyGenerationService;
    private readonly IProjectStatisticsManager _projectStatisticsManager;
    private readonly IAiSummaryIdeas _aiSummaryIdeas;
    private readonly IGoogleCloudStorageService _googleCloudStorageService;

    private const string InfoKey = "CreateProject_Info";
    private const string SurveyKey = "CreateProject_Survey";
    private const string IdeationKey = "CreateProject_Ideation";
    private const string DraftProjectIdKey = "CreateProject_DraftProjectId";

    public SubAdminProjectsController(
        ISubplatformManager subplatformManager,
        IProjectManager projectManager,
        UserManager<ApplicationUser> userManager,
        IManager manager,
        IImageGenerationService imageGenerationService,
        IIntroTextService introTextService,
        IAiSurveyGenerationService aiSurveyGenerationService,
        IProjectStatisticsManager projectStatisticsManager,
        IAiSummaryIdeas aiSummaryIdeas,
        IGoogleCloudStorageService googleCloudStorageService)
    {
        _subplatformManager = subplatformManager;
        _projectManager = projectManager;
        _userManager = userManager;
        _imageGenerationService = imageGenerationService;
        _introTextService = introTextService;
        _aiSurveyGenerationService = aiSurveyGenerationService;
        _projectStatisticsManager = projectStatisticsManager;
        _aiSummaryIdeas = aiSummaryIdeas;
        _googleCloudStorageService = googleCloudStorageService;
    }

    private string Subplatform
    {
        get
        {
            var fromRoute = RouteData.Values["subplatform"]?.ToString();
            return !string.IsNullOrWhiteSpace(fromRoute)
                ? fromRoute
                : (HttpContext.Items["subplatform"]?.ToString() ?? "");
        }
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

    private T GetSession<T>(string key)
    {
        var json = HttpContext.Session.GetString(key);
        return string.IsNullOrWhiteSpace(json) ? default : JsonSerializer.Deserialize<T>(json);
    }
    
    [HttpGet]
    public async Task<IActionResult> NewProject(string subplatform)
    {
        var errorResult = await ValidateSubplatformAccess(subplatform);
        if (errorResult != null) return errorResult;

        ClearProjectSessions();

        return RedirectToAction(nameof(ProjectInfo), new { subplatform });
    }
    
    [HttpGet]
    public async Task<IActionResult> ProjectInfo()
    {
        var subplatform = Subplatform;
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
    public async Task<IActionResult> Create(CreateProjectInfoViewModel vm)
    {
        var subplatform = Subplatform;
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

            vm.PhotoUri = await _googleCloudStorageService.UploadProjectImageAsync(
                vm.PhotoUpload,
                subplatform
            );
        }
        vm.PhotoUpload = null;
        SaveSession(InfoKey, vm);
      
        
        return await TryCreateProject(subplatform);
    }

    [HttpGet]
    public async Task<IActionResult> CreateSurvey()
    {
        var subplatform = Subplatform;
        var errorResult = await ValidateSubplatformAccess(subplatform);
        if (errorResult != null) return errorResult;

        var vm = GetSession<CreateProjecSurveyViewModel>(SurveyKey) ?? new CreateProjecSurveyViewModel();
        vm.SubplatformSlug = subplatform;

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> CreateIdeation()
    {
        var subplatform = Subplatform;
        var errorResult = await ValidateSubplatformAccess(subplatform);
        if (errorResult != null) return errorResult;

        var vm = GetSession<CreateProjectIdeationViewModel>(IdeationKey) ?? new CreateProjectIdeationViewModel();
        vm.SubplatformSlug = subplatform;

        vm.Topics ??= new List<IdeationTopicViewModel>();

        if (!vm.Topics.Any())
            vm.Topics.Add(new IdeationTopicViewModel());

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveIdeation(CreateProjectIdeationViewModel vm)
    {
        var subplatform = Subplatform;
        var errorResult = await ValidateSubplatformAccess(subplatform);
        if (errorResult != null) return errorResult;

        vm.SubplatformSlug = subplatform;

        vm.Topics = vm.Topics
            .Where(t => !string.IsNullOrWhiteSpace(t.Title))
            .ToList();

        if (!vm.Topics.Any())
            ModelState.AddModelError("", "Je moet minstens 1 topic invullen.");

        if (!ModelState.IsValid)
            return View("CreateIdeation", vm);

        SaveSession(IdeationKey, vm);

        return await TryCreateProject(subplatform);
    }

    [HttpGet]
    public async Task<IActionResult> EditDraft(int projectId)
    {
        var subplatform = Subplatform;
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

        SaveSession(SurveyKey, BuildSurveyViewModelFromProject(project, subplatform));

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

        return RedirectToAction(nameof(ProjectInfo));
    }

    [HttpGet]
    public async Task<IActionResult> CopyAsStartPoint(int projectId)
    {
        var subplatform = Subplatform;
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

        SaveSession(SurveyKey, BuildSurveyViewModelFromProject(project, subplatform));

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

        return RedirectToAction(nameof(ProjectInfo));
    }

    private CreateProjecSurveyViewModel BuildSurveyViewModelFromProject(Project project, string subplatform)
    {
        return new CreateProjecSurveyViewModel
        {
            SubplatformSlug = subplatform,
            Sections = project.QuestionList?.Sections
                .OrderBy(s => s.Order)
                .Select(s =>
                {
                    var conditionalFollowUpQuestionIds = s.Questions
                        .SelectMany(q => q.ConditionalQuestions ?? new List<ConditionalQuestion>())
                        .Where(cq => cq.FollowUpQuestion != null)
                        .Select(cq => cq.FollowUpQuestion.Id)
                        .ToHashSet();

                    return new SectionViewModel
                    {
                        Title = s.Title,
                        Order = s.Order,
                        Questions = s.Questions
                            .Where(q => !conditionalFollowUpQuestionIds.Contains(q.Id))
                            .Select(q => new QuestionViewModel
                            {
                                Description = q.Description,
                                QuestionType = q.QuestionType,
                                RangeMin = q.RangeMin,
                                RangeMax = q.RangeMax,
                                Options = q.Options?
                                    .Select(o => o.Text)
                                    .ToList() ?? new List<string>(),

                                Conditionals = q.ConditionalQuestions?
                                    .Where(cq => cq.FollowUpQuestion != null)
                                    .Select(cq => new ConditionalQuestionViewModel
                                    {
                                        Trigger = cq.TriggerValue,
                                        QuestionText = cq.FollowUpQuestion.Description,
                                        UseAi = false
                                    })
                                    .ToList() ?? new List<ConditionalQuestionViewModel>()
                            })
                            .ToList()
                    };
                })
                .ToList() ?? new List<SectionViewModel>()
        };
    }

    private async Task<IActionResult> TryCreateProject(string subplatform)
    {
        var validationResult = ValidateProjectSessions();
        if (validationResult != null) return validationResult;

        var info = GetSession<CreateProjectInfoViewModel>(InfoKey)!;
        var survey = GetSession<CreateProjecSurveyViewModel>(SurveyKey)!;
        var ideation = GetSession<CreateProjectIdeationViewModel>(IdeationKey)!;

        var subPlatform = _subplatformManager.GetSubPlatformBySlug(subplatform);
        if (subPlatform == null) return NotFound();

        var draftProjectId = GetSession<int?>(DraftProjectIdKey);

        if (draftProjectId.HasValue)
            return await UpdateExistingDraft(draftProjectId.Value, info, survey, ideation, subplatform);

        var project = await BuildProject(info, survey, ideation, subPlatform.Id);

        _projectManager.CreateProject(project);

        ClearProjectSessions();

        return RedirectToAction("Index", "SubAdmin");
    }

    private IActionResult ValidateProjectSessions()
    {
        if (GetSession<CreateProjectInfoViewModel>(InfoKey) == null)
        {
            TempData["ProjectError"] = "Vul eerst Project Info volledig in.";
            return RedirectToAction(nameof(ProjectInfo));
        }

        if (GetSession<CreateProjecSurveyViewModel>(SurveyKey) == null)
        {
            TempData["ProjectError"] = "Vul eerst Bevraging volledig in.";
            return RedirectToAction(nameof(CreateSurvey));
        }

        if (GetSession<CreateProjectIdeationViewModel>(IdeationKey) == null)
        {
            TempData["ProjectError"] = "Vul eerst Ideation volledig in.";
            return RedirectToAction(nameof(CreateIdeation));
        }

        return null;
    }

    private async Task<Project> BuildProject(
        CreateProjectInfoViewModel info,
        CreateProjecSurveyViewModel survey,
        CreateProjectIdeationViewModel ideation,
        int subPlatformId)
    {
        var photoUri = info.PhotoUri;

        if (string.IsNullOrWhiteSpace(photoUri))
        {
            photoUri = await _imageGenerationService.GenerateProjectImageAsync(
                info.Name,
                info.Introduction
            );
        }

        return new Project
        {
            Name = info.Name,
            Introduction = info.Introduction,
            Type = info.Type,
            Status = Status.Draft,
            ReactionEmojiGroup = ideation.SelectedEmojiGroup,
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
        var questionList = new QuestionList
        {
            Sections = new List<Section>()
        };

        foreach (var sectionVm in survey.Sections.Select((s, i) => new { Section = s, Index = i }))
        {
            var section = new Section
            {
                Title = sectionVm.Section.Title,
                Order = sectionVm.Index + 1,
                Questions = new List<Question>()
            };

            foreach (var questionVm in sectionVm.Section.Questions)
            {
                var parentQuestion = new Question
                {
                    Description = questionVm.Description,
                    QuestionType = questionVm.QuestionType,
                    RangeMin = questionVm.RangeMin,
                    RangeMax = questionVm.RangeMax,
                    Options = questionVm.Options
                        .Where(o => !string.IsNullOrWhiteSpace(o))
                        .Select(o => new QuestionOption { Text = o })
                        .ToList(),
                    ConditionalQuestions = new List<ConditionalQuestion>()
                };

                section.Questions = section.Questions.Append(parentQuestion).ToList();

                foreach (var conditionalVm in questionVm.Conditionals)
                {
                    if (string.IsNullOrWhiteSpace(conditionalVm.Trigger))
                        continue;

                    var followUpQuestion = new Question
                    {
                        Description = string.IsNullOrWhiteSpace(conditionalVm.QuestionText)
                            ? "Waarom?"
                            : conditionalVm.QuestionText,
                        QuestionType = QuestionType.OpenQuestion,
                        Options = new List<QuestionOption>(),
                        IsRequired = false
                    };

                    section.Questions = section.Questions.Append(followUpQuestion).ToList();

                    parentQuestion.ConditionalQuestions.Add(new ConditionalQuestion
                    {
                        ParentQuestion = parentQuestion,
                        FollowUpQuestion = followUpQuestion,
                        TriggerValue = conditionalVm.Trigger,
                        TriggerType = TriggerType.Contains
                    });
                }
            }

            questionList.Sections.Add(section);
        }

        return questionList;
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

        var photoUri = info.PhotoUri;

        if (string.IsNullOrWhiteSpace(photoUri))
        {
            photoUri = await _imageGenerationService.GenerateProjectImageAsync(
                info.Name,
                info.Introduction
            );
        }

        existingProject.Name = info.Name;
        existingProject.Introduction = info.Introduction;
        existingProject.Type = info.Type;
        existingProject.ReactionEmojiGroup = ideation.SelectedEmojiGroup;
        existingProject.Photo = !string.IsNullOrWhiteSpace(photoUri)
            ? new Media { Uri = photoUri }
            : null;
        existingProject.Topics = BuildTopics(ideation);
        existingProject.QuestionList = BuildQuestionList(survey);

        _projectManager.UpdateProject(existingProject);

        ClearProjectSessions();

        return RedirectToAction("Index", "SubAdmin");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("ai-limit")]
    public async Task<IActionResult> GenerateProjectImage(
        string subplatform,
        [FromBody] GenerateProjectImageRequest request)
    {
        var errorResult = await ValidateSubplatformAccess(Subplatform);
        if (errorResult != null) return errorResult;

        if (request == null || string.IsNullOrWhiteSpace(request.ProjectName))
        {
            return BadRequest(new
            {
                ok = false,
                message = "Projectnaam is verplicht."
            });
        }

        var imageUrl = await _imageGenerationService.GenerateProjectImageAsync(
            request.ProjectName,
            request.Introduction
        );

        return Ok(new
        {
            ok = true,
            imageUrl
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateSurvey([FromBody] GenerateSurveyRequest request)
    {
        var errorResult = await ValidateSubplatformAccess(Subplatform);
        if (errorResult != null) return errorResult;

        if (request == null || string.IsNullOrWhiteSpace(request.Description))
        {
            return BadRequest(new
            {
                ok = false,
                message = "Beschrijving is verplicht."
            });
        }

        var survey = await _aiSurveyGenerationService.GenerateSurveyAsync(request.Description, request.QuestionAmount);

        return Ok(new
        {
            ok = true,
            survey,
            message = "Vragenlijst gegenereerd."
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateIntroduction([FromBody] GenerateIntroductionRequest request)
    {
        var errorResult = await ValidateSubplatformAccess(Subplatform);
        if (errorResult != null) return errorResult;

        if (request == null || string.IsNullOrWhiteSpace(request.ProjectName))
        {
            return BadRequest(new
            {
                ok = false,
                message = "Projectnaam is verplicht."
            });
        }

        var introduction = await _introTextService.GenerateIntroAsync(request.ProjectName);

        return Ok(new
        {
            ok = true,
            introduction
        });
    }
    
    [HttpGet]
    public async Task<IActionResult> Statistics(string subplatform, int projectId)
    {
        var errorResult = await ValidateSubplatformAccess(subplatform);
        if (errorResult != null) return errorResult;

        var project = _projectManager.GetProject(projectId);

        if (project == null || project.SubPlatform.Slug != subplatform)
        {
            return NotFound();
        }

        var statistics = _projectStatisticsManager.GetProjectStatistics(projectId);

        return View(statistics);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateOpenQuestionSummary(
        string subplatform,
        int projectId,
        int questionId)
    {
        var errorResult = await ValidateSubplatformAccess(subplatform);
        if (errorResult != null) return errorResult;

        var project = _projectManager.GetProject(projectId);

        if (project == null || project.SubPlatform.Slug != subplatform)
        {
            return NotFound();
        }

        await _aiSummaryIdeas.GenerateOpenQuestionSummaryAsync(projectId, questionId);

        return RedirectToAction(nameof(Statistics), new
        {
            subplatform,
            projectId
        });
    }

    public class GenerateProjectImageRequest
    {
        public string ProjectName { get; set; } = string.Empty;
        public string Introduction { get; set; } = string.Empty;
    }

    public class GenerateIntroductionRequest
    {
        public string ProjectName { get; set; } = "";
        public string Description { get; set; } = "";
    }

    private void ClearProjectSessions()
    {
        HttpContext.Session.Remove(InfoKey);
        HttpContext.Session.Remove(SurveyKey);
        HttpContext.Session.Remove(IdeationKey);
        HttpContext.Session.Remove(DraftProjectIdKey);
    }
}