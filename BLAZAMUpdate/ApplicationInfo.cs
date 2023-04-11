using BLAZAM.FileSystem;
using Microsoft.AspNetCore.Builder;
using System.Diagnostics;

namespace BLAZAM.Update
{
    public class ApplicationInfo
    {
        public ApplicationVersion RunningVersion { get; set; }
        public Process RunningProcess { get; set; }
        public SystemDirectory ApplicationRoot { get; set; }
        public SystemDirectory TempDirectory { get; set; }

        public ApplicationInfo() {
        
        }
        public ApplicationInfo(WebApplicationBuilder builder) {
            RunningProcess = Process.GetCurrentProcess();
            ApplicationRoot= new SystemDirectory(builder.Environment.ContentRootPath);
            TempDirectory = new SystemDirectory(Path.GetTempPath() + "Blazam\\");
            //AppDataDirectory = new SystemDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Blazam\\");
        } 
    }
}