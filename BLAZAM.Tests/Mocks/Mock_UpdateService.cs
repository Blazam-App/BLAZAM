using System.Diagnostics;
using BLAZAM.FileSystem;
using BLAZAM.Global.Data;
using BLAZAM.Update;
using BLAZAM.Update.Services;

namespace BLAZAM.Tests.Mocks
{
    internal class Mock_UpdateService : UpdateService
    {
        public Mock_UpdateService() : base(new()
        {
            ApplicationRoot = new SystemDirectory("C:\\temp"),
            RunningProcess = Process.GetCurrentProcess(),
            RunningVersion = new ApplicationVersion("0.0.1"),
            TempDirectory = new SystemDirectory("C:\\temp")
        }, null)
        {

            SelectedBranch = ApplicationReleaseBranches.Stable;
        }
    }
}
