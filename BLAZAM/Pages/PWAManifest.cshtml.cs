using BLAZAM.Database.Context;
using BLAZAM.Static;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace BLAZAM.Pages
{
    public class ManifestIcon
    {
        public string src;
        public string sizes;
        public string type;
    }
    public class PWAManifest
    {
        public string short_name = "Blazam";
        public string name = "Blazam";
        public List<ManifestIcon> icons= new List<ManifestIcon>();
        public string start_url = ".";
        public string display = "minimal-ui";
        public string theme_color = "#000000";
        public string background_color = "#FFFFFF";
        public string description = "The modern Active Directory management tool.";

    }
    [Produces("application/json")]
    public class PWAManifestModel : PageModel
    {
        private readonly IAppDatabaseFactory _factory;

        public PWAManifestModel(IAppDatabaseFactory factory)
        {
            _factory  = factory;
        }

        public async Task<IActionResult> OnGet()
        {
            var context = await _factory.CreateDbContextAsync();
            var manifest = new PWAManifest();
            var icon = new ManifestIcon();
            icon.src = @StaticAssets.ApplicationIconUri;
            icon.sizes = "250x250";
            icon.type = "image/png";
            manifest.icons.Add(icon);
            try
            {
                manifest.short_name = context.AppSettings.FirstOrDefault()?.AppAbbreviation;
                manifest.name = context.AppSettings.FirstOrDefault()?.AppName;
            }
            catch
            {

            }
            return Content(JsonConvert.SerializeObject(manifest));


        }
    }
}
