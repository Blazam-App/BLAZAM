using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IO.Compression;

namespace BLAZAM.Server.Pages.Download
{
    public class LogsModel : PageModel
    {

        public IActionResult OnGet()
        {
            var inMemZip = GenerateZip();
            return File(inMemZip, "application/zip");
        }

        private byte[] GenerateZip()
        {
            byte[] zipBytes;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (ZipArchive zip = new ZipArchive(memoryStream, ZipArchiveMode.Create))
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
