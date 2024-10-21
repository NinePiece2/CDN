using Microsoft.AspNetCore.Mvc;

namespace CDN.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
