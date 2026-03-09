using IntergratieProject.BL;
using Microsoft.AspNetCore.Mvc;

namespace IntergratieProject.UI.MVC.Controllers
{
    public class ChatController : Controller
    {
        private readonly IManager _manager;

        public ChatController(IManager manager)
        {
            _manager = manager;
        }

        // toont de chatpagina
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Send(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return BadRequest(new { ok = false, error = "Empty message" });

            var result = await _manager.ModerateTextAsync(message);

            if (result.IsToxic && string.IsNullOrWhiteSpace(result.SuggestedText))
            {
                return Ok(new {
                    ok = true,
                    isToxic = true,
                    warning = "Moderatie kon niet uitgevoerd worden. Probeer opnieuw of pas je tekst aan.",
                    explanation = result.Explanation,
                    suggestedText = ""
                });
            }

            // toxisch -> geef voorstel terug
            return Ok(new {
                ok = true,
                isToxic = true,
                warning = "De tekst bevat toxische woorden. Het kan zijn dat de tekst niet goedgekeurd wordt.",
                explanation = result.Explanation,
                suggestedText = result.SuggestedText
            });
        }
    }
}