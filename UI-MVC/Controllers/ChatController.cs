using IntergratieProject.BL;
using Microsoft.AspNetCore.Mvc;

namespace UI_MVC.Controllers
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

        // ontvangt chat bericht
        [HttpPost]
        public async Task<IActionResult> Send(string message)
        {
            var response = await _manager.AskAiForIdea(message);

            return Ok(response);
        }
    }
}