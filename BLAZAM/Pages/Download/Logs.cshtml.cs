using System.IO.Compression;
using BLAZAM.Common.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BLAZAM.Server.Pages.Download
{
    /// <summary>
    /// Log controller for exporting application logs
    /// </summary>
    [Authorize(Roles = UserRoles.SuperAdmin)]
    public class LogsModel : PageModel
    {
        /// <summary>
        /// On get return the zip of the logs
        /// </summary>
        /// <returns></returns>

        public IActionResult OnGet()
        {
            var inMemZip = GenerateZip();
            return File(inMemZip, "application/zip");
        }

        private static byte[] GenerateZip()
        {
            using (MemoryStream memoryStream = new())
            {
                using (ZipArchive zip = new(memoryStream, ZipArchiveMode.Create))
                {
                    var logPath = Loggers.LogPath;
                    // Recursively add files and subdirectories to the zip archive
                    zip.AddToZip(new SystemDirectory(logPath), logPath);
                }
                return memoryStream.ToArray();
            }
        }
    }
}
