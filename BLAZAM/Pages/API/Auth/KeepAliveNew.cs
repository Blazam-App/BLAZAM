using Microsoft.AspNetCore.Mvc;

namespace BLAZAM.Server.Pages.API.Auth
{
    public class KeepAliveNew : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
