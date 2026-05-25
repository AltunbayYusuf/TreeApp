using System.Diagnostics;
using IntegratieProject.BL.Domain.project;
using IntegratieProject.BL.interfaces;
using IntegratieProject.UI.MVC.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace IntegratieProject.UI.MVC.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IProjectManager _projectManager;

    public HomeController(ILogger<HomeController> logger,  IProjectManager projectManager)
    {
        _logger = logger;
        _projectManager = projectManager;
    }

    private string Subplatform => HttpContext.Items["subplatform"]?.ToString() ?? "";

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy(int? projectId)
    {
        if (projectId.HasValue)
        {
            var project = _projectManager.GetProjectBySubPlatformAndProjectId(Subplatform, projectId.Value);
            ViewBag.ProjectFontFamily = project?.FontFamily;
        }

        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult NotFound()
    {
        Response.StatusCode = 404;

        var subplatform = Subplatform;
        if (string.IsNullOrWhiteSpace(subplatform))
            return View();

        var reExecute = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
        if (reExecute == null)
            return View();

        var projectId = ExtractProjectId(reExecute.OriginalPath, reExecute.OriginalQueryString);
        if (projectId == null)
            return View();

        var project = _projectManager.GetProjectBySubPlatformAndProjectId(subplatform, projectId.Value);
        if (project != null && project.Status == Status.Active)
        {
            ViewBag.HomeProjectId = projectId.Value;
            ViewBag.HomeSubplatform = subplatform;
        }

        return View();
    }

    private static int? ExtractProjectId(string path, string queryString)
    {
        // Check query string: ?projectId=x or ?id=x
        if (!string.IsNullOrEmpty(queryString))
        {
            var qs = queryString.TrimStart('?');
            foreach (var part in qs.Split('&'))
            {
                var kv = part.Split('=');
                if (kv.Length == 2 &&
                    (kv[0].Equals("projectId", StringComparison.OrdinalIgnoreCase) ||
                     kv[0].Equals("id", StringComparison.OrdinalIgnoreCase)) &&
                    int.TryParse(kv[1], out var qid))
                    return qid;
            }
        }

        // Check path segments: last numeric segment is the id
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        for (var i = segments.Length - 1; i >= 0; i--)
        {
            if (int.TryParse(segments[i], out var sid))
                return sid;
        }

        return null;
    }
    
}