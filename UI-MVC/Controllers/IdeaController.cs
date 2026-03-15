using IntergratieProject.BL;
using IntergratieProject.Domain.project;
using IntergratieProject.Models;
using IntergratieProject.UI.MVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace IntergratieProject.UI.MVC.Controllers;

public class IdeaController : Controller
{
    private readonly IManager _manager;

    public IdeaController(IManager manager)
    {
        _manager = manager;
    }


    public IActionResult Index(int? topicId)
    {
        Project project = (getCurrentProject());

        var vm = new IdeasOverviewViewModel()
        {
            Project = project,
            SelectedTopicId = topicId,
            Topics = _manager.GetTopicsByProject(project),
            Ideas = _manager.GetIdeasByProject(project, topicId)
        };
        return View(vm);
    }
    
    [HttpGet]
    public IActionResult Create()
    {
        Project project = getCurrentProject();

        var vm = new IdeasOverviewViewModel
        {
            Project = project,
            Topics = _manager.GetTopicsByProject(project),
            Ideas = _manager.GetIdeasByProject(project)
        };

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> SubmitIdea([FromForm] SubmitIdeaViewModel vm)
    {
        if (vm.TopicId <= 0)
        {
            return Json(new
            {
                ok = false,
                message = "Kies eerst een topic."
            });
        }

        if (string.IsNullOrWhiteSpace(vm.Text))
        {
            return Json(new
            {
                ok = false,
                message = "Tekst mag niet leeg zijn."
            });
        }

        var result = await _manager.SubmitIdeaAsync(vm.TopicId, vm.Title, vm.Text);

        if (result.IsToxic)
        {
            return Json(new
            {
                ok = true,
                saved = false,
                isToxic = true,
                warning = "Deze tekst bevat toxische inhoud en werd niet verzonden.",
                explanation = result.Explanation,
                suggestedText = result.SuggestedText
            });
        }

        return Json(new
        {
            ok = true,
            saved = true,
            isToxic = false,
            message = "Idee succesvol verstuurd."
        });
    }
    
    [HttpPost]
    public async Task<IActionResult> ForceSubmitIdea([FromForm] SubmitIdeaViewModel vm)
    {
        if (vm.TopicId <= 0)
        {
            return Json(new
            {
                ok = false,
                message = "Kies eerst een topic."
            });
        }

        if (string.IsNullOrWhiteSpace(vm.Text))
        {
            return Json(new
            {
                ok = false,
                message = "Beschrijving mag niet leeg zijn."
            });
        }

        await _manager.ForceSubmitIdeaAsync(vm.TopicId, vm.Title, vm.Text);

        return Json(new
        {
            ok = true,
            saved = true,
            isToxic = false,
            message = "Idee doorgestuurd voor moderatie."
        });
    }

    private Project getCurrentProject()
    {
        return new Project()
        {
            Id = 1
        };
    }
}