using BLAZAM.Common;
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
            return File(inMemZip.ToArray(), "application/zip");
        }

        private MemoryStream GenerateZip()
        {
            MemoryStream memoryStream = new MemoryStream();
            ZipArchive zip = new ZipArchive(memoryStream, ZipArchiveMode.Create);
            // Recursively add files and subdirectories to the zip archive
            //TODO make zip file
            // zip.AddToZip(new SystemDirectory(LogPath),LogPath);

            return memoryStream;
        }
    }
}
