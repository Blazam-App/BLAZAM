using BLAZAM.Common.Data;
using BLAZAM.Database.Context;
using BLAZAM.Jobs;
using BLAZAM.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.Localization;
using Polly;
using System.IO.Compression;

namespace BLAZAM.Server.Pages.Download
{
    /// <summary>
    /// Log controller for exporting application logs
    /// </summary>
    [Authorize(Roles = UserRoles.SuperAdmin)]
    public class DBExport : PageModel
    {
        private readonly IStringLocalizer<AppLocalization> _appLocalization;
        private readonly IAppDatabaseFactory _factory;

        public DBExport(IStringLocalizer<AppLocalization> appLocalization, IAppDatabaseFactory factory)
        {
            _appLocalization = appLocalization;
            _factory = factory;
        }



        /// <summary>
        /// On get return the zip of the logs
        /// </summary>
        /// <returns></returns>

        public IActionResult OnGet()
        {
            var inMemZip = GenerateZip();
            return File(inMemZip, "application/zip");
        }

        private byte[] GenerateZip()
        {
            var tempPath = Path.Combine(Program.WritablePath.FullPath, "export");
            using (MemoryStream memoryStream = new())
            {
                using (ZipArchive zip = new(memoryStream, ZipArchiveMode.Create))
                {
                    Job exportJob = new Job(_appLocalization["Export Database"], User.Identity.Name);
                    JobStep exportData = new JobStep(_appLocalization["Export Data"], (step) =>
                    {
                        var context = _factory.CreateDbContext();
                        context.Export(tempPath);
                        return true;

                    });
                    JobStep packageData = new JobStep(_appLocalization["Prepare Files"], (step) =>
                    {

                        var exportDir = new SystemDirectory(tempPath);
                        zip.AddToZip(exportDir, exportDir.FullPath);
                        return true;

                    });
                    exportJob.AddStep(exportData);
                    exportJob.AddStep(packageData);
                    exportJob.Run();

                }
                return memoryStream.ToArray();
            }





        }
    }
}
