namespace BLAZAM.Common.Data
{
    public static class PrerequisiteChecker
    {
        public static bool CheckForAspCore()
        {
            try
            {
                string? sharedFrameworkPath = GetSharedFrameworkPath();
                if (string.IsNullOrEmpty(sharedFrameworkPath))
                {
                    return false;
                }

                if (!Directory.Exists(sharedFrameworkPath))
                {
                    return false;
                }

                var dirs = Directory.GetDirectories(sharedFrameworkPath);
                if (dirs == null || dirs.Length == 0)
                {
                    return false;
                }

                return dirs.Any(dir => dir.Contains("8."));
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex, "Error checking for ASP.NET Core prerequisites.");
            }
            return false;
        }

        private static string? GetSharedFrameworkPath()
        {
            if (OperatingSystem.IsWindows())
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    "dotnet", "shared", "Microsoft.NETCore.App"
                );
            }
            if (OperatingSystem.IsLinux())
            {
                var possiblePaths = new[]
                {
                    "/usr/share/dotnet/shared/Microsoft.NETCore.App",
                    "/usr/local/share/dotnet/shared/Microsoft.NETCore.App"
                };
                return possiblePaths.FirstOrDefault(Directory.Exists)
                    ?? possiblePaths[0];
            }
            return null;
        }

        public static bool CheckForAspCoreHosting()
        {
            try
            {
                var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\Microsoft\\Updates\\.NET\\");
                if (key != null)
                {
                    var possibleAspKeys = key.GetSubKeyNames();
                    if (possibleAspKeys.Length > 0)
                    {
                        foreach (var possibleKey in possibleAspKeys)
                        {
                            if (possibleKey.Contains("Microsoft .NET 8") && possibleKey.Contains("Hosting"))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex, "Error checking for ASP.NET Core Hosting prerequisites.");
            }
            return false;
        }
    }
}
