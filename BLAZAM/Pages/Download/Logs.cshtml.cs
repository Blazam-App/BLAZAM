using BLAZAM.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BLAZAM.Server.Pages.Download
{
    public class LogsModel : PageModel
    {
        public IActionResult OnGet()
        {
              var inMemZip = Loggers.GenerateZip();
              return File(inMemZip.ToArray(),"application/zip");
        }
    }
}
