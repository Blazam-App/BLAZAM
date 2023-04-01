using BLAZAM.Common.Data.Database;
using BLAZAM.Common.Extensions;
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
        public  IDatabaseContext Context { get; private set; }
        public StaticModel(AppDatabaseFactory factory)
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
    public class StaticAssets
    {
        public static string ApplicationIconUri = "/static/img/appicon.png";
        public static string FaviconUri = "/static/img/favicon.ico";

        public static byte[] AppIcon(int size = 250)
        {

            var dbIcon = DatabaseCache.AppIcon;
            if (dbIcon != null)
            {
                return dbIcon.ReizeRawImage(size);
            }
            else
            {
                var defIcon = GetDefaultIcon();
                if (defIcon != null)
                {
                    return defIcon.ReizeRawImage(size);
                }
            }
            return null;
        }


        private static byte[]? GetDefaultIcon()
        {
            var defaultIconFilePath = Path.GetFullPath(Program.RootDirectory + @"static\img\default_logo2.png");
            if (System.IO.File.Exists(defaultIconFilePath))
                return System.IO.File.ReadAllBytes(defaultIconFilePath);
            return null;
        }
    }
}
