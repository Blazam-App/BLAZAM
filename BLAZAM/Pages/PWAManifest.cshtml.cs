using System.Data.Entity;
using BLAZAM.Database.Context;
using BLAZAM.Static;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace BLAZAM.Pages
{

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class ManifestIcon
    {
        public string src { get; set; }
        public string sizes { get; set; }
        public string type { get; set; }
    }
    public class PWAManifest
    {
        public string short_name { get; set; } = "Blazam";
        public string name { get; set; } = "Blazam";
        public List<ManifestIcon> icons { get; set; } = new();
        public string start_url { get; set; } = ".";
        public string display { get; set; } = "minimal-ui";
        public string theme_color { get; set; } = "#000000";
        public string background_color { get; set; } = "#FFFFFF";
        public string description { get; set; } = "The modern Active Directory management tool.";

    }
    [Produces("application/json")]
    public class PWAManifestModel : PageModel
    {
        private readonly IUserDatabaseFactory _factory;

        public PWAManifestModel(IUserDatabaseFactory factory)
        {
            _factory = factory;
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
                var appSettings = await context.AppSettings.FirstOrDefaultAsync();
                if (appSettings != null)
                {
                    manifest.short_name = appSettings.AppAbbreviation;
                    manifest.name = appSettings.AppName;
                }
            }
            catch
            {

            }
            return Content(JsonConvert.SerializeObject(manifest));


        }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

}
