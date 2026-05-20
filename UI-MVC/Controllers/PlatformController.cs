using IntegratieProject.BL.interfaces;
using IntegratieProject.UI.MVC.Models;
using IntegratieProject.UI.MVC.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace IntegratieProject.UI.MVC.Controllers;

[Authorize(Roles = CustomIdentityConstants.GeneralAdminRoleName)]
public class PlatformController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ISubplatformManager _subplatformManager;

    public PlatformController(ILogger<HomeController> logger, ISubplatformManager subplatformManager)
    {
        _logger = logger;
        _subplatformManager = subplatformManager;
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        var subPlatform = _subplatformManager.GetSubPlatform(id);

        if (subPlatform == null) return NotFound();

        return View(subPlatform);
    }

    [HttpGet]
    public IActionResult ExportSubAdminsCsv(int id)
    {
        var subPlatform = _subplatformManager.GetSubPlatform(id);

        if (subPlatform == null) return NotFound();

        var allResponses = subPlatform.Projects
            .SelectMany(p => p.SurveyResponses ?? [])
            .ToList();

        var avgSeconds = allResponses.Any()
            ? (int)Math.Round(allResponses.Average(r => r.DurationInSeconds))
            : 0;

        var avgMinutes = avgSeconds / 60;
        var remainingSeconds = avgSeconds % 60;

        var csv = new StringBuilder();
        csv.AppendLine("sep=;");
        csv.AppendLine("Subplatform");
        csv.AppendLine($"Naam;{EscapeCsvValue(subPlatform.CompanyName)}");
        csv.AppendLine();

        csv.AppendLine("Subadmins");

        foreach (var subAdmin in subPlatform.SubAdmins)
        {
            csv.AppendLine(EscapeCsvValue(subAdmin.Name));
        }

        csv.AppendLine();
        csv.AppendLine("Subplatform overzicht");
        csv.AppendLine($"Projecten;{subPlatform.Projects.Count}");
        csv.AppendLine($"Deelnemers;{subPlatform.Projects.Sum(p => p.SurveyResponses?.Count ?? 0)}");
        csv.AppendLine($"Gem. tijd;{avgMinutes} min {remainingSeconds} sec");
        csv.AppendLine("Totale kosten;");

        csv.AppendLine();
        csv.AppendLine("Projecten");
        csv.AppendLine("Naam;Deelnemers");

        foreach (var project in subPlatform.Projects)
        {
            csv.AppendLine($"{EscapeCsvValue(project.Name)};{project.SurveyResponses?.Count ?? 0}");
        }

        var fileName = $"subplatform-{subPlatform.Slug}.csv";

        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", fileName);
    }

    private static string EscapeCsvValue(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;

        var escapedValue = value.Replace("\"", "\"\"");
        return escapedValue.Contains(';') || escapedValue.Contains('"') || escapedValue.Contains('\n') || escapedValue.Contains('\r')
            ? $"\"{escapedValue}\""
            : escapedValue;
    }

    [HttpPost]
    public async Task<IActionResult> CreateSubPlatform([FromBody]CreateSubPlatformDto dto) 
    {
        if (!ModelState.IsValid)
            return BadRequest("Ongeldige invoer");

        try
        {
            var generatedPassword = await _subplatformManager.CreateSubPlatformAsync(
                dto.CompanyName,
                dto.Slug,
                dto.AdminEmail,
                dto.LogoFile 
            );

            return Ok(new { password = generatedPassword });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
