using BLAZAM.Common.Data.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Reflection.Metadata;

namespace BLAZAM.Server.Pages
{
    public class StaticAssets : PageModel
    {

        public static string ApplicationIcon = "/static/img/appicon.png";
        public static string Favicon = "/static/img/favicon.ico";





        [BindProperty(SupportsGet = true)]
        public string Method { get; set; }
        [BindProperty(SupportsGet = true)]
        public string Data { get; set; }
        public DatabaseContext Context { get; private set; }




        public StaticAssets(IDbContextFactory<DatabaseContext> factory)
        {
            Context = factory.CreateDbContext();

        }

        public async Task<IActionResult> OnGet()
        {
            return await Task.Run(() =>
            {

                var expires = DateTime.UtcNow.AddDays(1);
                Response.Headers.Add("Cache-Control", "public,max-age=86400");
                Response.Headers.Add("Expires", expires.ToString("R"));
                switch (Method.ToLower())
                {
                    case "img":
                        return GetImage(Data);
                   

                }
                return null;

            });

        }
        private byte[] AppIcon(int size = 250)
        {

            return DatabaseCache.AppIcon.ReizeRawImage(size);
        }
        private IActionResult GetImage(string data)
        {
            switch (data.ToLower())
            {
                case "appicon.png":
                    return File(AppIcon(), "image/png");
                case "favicon.ico":
                    return File(AppIcon(50), "image/x-icon");
            }
            return null;

        }
    }

  
}
