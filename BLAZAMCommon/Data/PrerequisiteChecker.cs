using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Common.Data
{
    public static class PrerequisiteChecker
    {
        public static bool CheckForAspCore()
        {
            try
            {
                var dirs = Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\dotnet\\shared\\Microsoft.NETCore.App");
                if (dirs != null && dirs.Length > 0)
                {

                    foreach (var dir in dirs)
                    {
                        if (dir.Contains("8."))
                        {


                            return true;
                        }
                    }

                }
            }
            catch { }
            return false;
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
            catch { }
            return false;

        }
    }
}
