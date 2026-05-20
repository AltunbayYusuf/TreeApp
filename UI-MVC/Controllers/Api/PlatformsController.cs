using IntegratieProject.BL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IntegratieProject.UI.MVC.Controllers.API;

[ApiController]
[Route("api/[controller]")]
public class AiUsageController : ControllerBase
{
    private readonly IAiUsageManager _aiUsageManager;

    public AiUsageController(IAiUsageManager aiUsageManager)
    {
        _aiUsageManager = aiUsageManager;
    }

    [HttpGet("total-cost")]
    public IActionResult GetTotalCost(int subPlatformId)
    {
        var totalCost = _aiUsageManager
            .GetAllUsages()
            .Where(x => x.SubPlatformId == subPlatformId)
            .Sum(x => x.EstimatedCost);

        return Ok(new
        {
            totalCost
        });
    }
}