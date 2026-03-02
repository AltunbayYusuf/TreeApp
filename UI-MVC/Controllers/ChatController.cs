using Microsoft.AspNetCore.Mvc;

namespace UI_MVC.Controllers
{
    public class ChatController : Controller
    {
        // GET: Chat
        public IActionResult Index()
        {
            return View();
        }
    }
}