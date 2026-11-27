using BLAZAM.Common.Data;
using BLAZAM.FileSystem;
using BLAZAM.Global.Data;
using BLAZAM.Update;
using BLAZAM.Update.Services;
using System.Diagnostics;

internal class Mock_UpdateService : UpdateService
{
    private static readonly string TestRoot = Path.Combine(Path.GetTempPath(), "blazam-tests");
    private static readonly string AppRootPath = Path.Combine(TestRoot, "app");
    private static readonly string TempPath = Path.Combine(TestRoot, "temp");

    public Mock_UpdateService() : base(CreateApplicationInfo(), null)
    {
        SelectedBranch = ApplicationReleaseBranches.Net8ReleasePrefix;
    }

    private static ApplicationInfo CreateApplicationInfo()
    {
        Directory.CreateDirectory(AppRootPath);
        Directory.CreateDirectory(TempPath);

        // Ensure ApplicationRoot has at least one file for Backup() tests
        var dummyFile = Path.Combine(AppRootPath, "test.txt");
        if (!File.Exists(dummyFile))
        {
            File.WriteAllText(dummyFile, "test");
        }

        return new ApplicationInfo
        {
            ApplicationRoot = new SystemDirectory(AppRootPath),
            RunningProcess = Process.GetCurrentProcess(),
            RunningVersion = new ApplicationVersion("0.0.1"),
            TempDirectory = new SystemDirectory(TempPath)
        };
    }
}