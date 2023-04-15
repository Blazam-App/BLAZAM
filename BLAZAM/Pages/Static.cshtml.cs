using BLAZAM.Common.Data.Database;
using BLAZAM.Database.Context;
using BLAZAM.Gui;
using BLAZAM.Static;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BLAZAM.Server.Pages
{
    public class StaticModel : PageModel
    {

        [BindProperty(SupportsGet = true)]
        public string Method { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Data { get; set; }


        protected  IDatabaseContext Context { get; private set; }


        public StaticModel(IAppDatabaseFactory factory)
        {
            Context = factory.CreateDbContext();

        }


        public async Task<IActionResult> OnGet()
        {

             var expires = DateTime.UtcNow.AddDays(1);
                Response.Headers.Add("Cache-Control", "public,max-age=86400");
                Response.Headers.Add("Expires", expires.ToString("R"));
                
            switch (Method.ToLower())
            {
                case "img":
                    return GetImg(Data);

                    break;
            }
            return null;

        }


        public IActionResult GetImg(string data)
        {
            switch (data.ToLower())
            {
                case "appicon.png":
                    return File(StaticAssets.AppIcon(), "image/png");
                case "favicon.ico":
                    return File(StaticAssets.AppIcon(50), "image/x-icon");
            }

            return null;
        }
  
    }
}
